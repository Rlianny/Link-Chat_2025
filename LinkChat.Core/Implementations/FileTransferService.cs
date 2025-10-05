using LinkChat.Core.Services;
using LinkChat.Core.Models;
using LinkChat.Core.Tools;
using System.Threading.Tasks;
using System.Drawing;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Text;
namespace LinkChat.Core.Implementations;

public class FileTransferService : IFileTransferService
{
    Dictionary<string, Models.File> Files = [];
    Dictionary<string, Dictionary<int, FileChunk>> FileChunks = [];
    Dictionary<string, FileStart> FileStarts = [];
    Dictionary<string, Dictionary<int, bool>> Confirmations = [];
    Dictionary<string, bool> ConfirmingStarts = [];
    private IProtocolService protocolService;
    private INetworkService networkService;
    private IUserService userService;

    public event Action<Models.File> FileFrameReceived;


    public FileTransferService(IProtocolService protocolService, INetworkService networkService, IUserService userService)
    {
        this.protocolService = protocolService;
        this.networkService = networkService;
        this.userService = userService;
        protocolService.FileStartFrameReceived += OnFileStartFrameReceived;
        protocolService.FileChunkFrameReceived += OnFileChunkFrameReceived;
        protocolService.FileChunkAckFrameReceived += OnFileChunkAckFrameReceived;
        protocolService.FileStartAckFrameReceived += OnFileStartAckFrameReceived;
    }
    public List<FileChunk> SplitFile(
        string filePath,
        string userName,
        int chunkSize = 800) // Reduced to account for protocol overhead
    {
        // Ensure chunk size doesn't exceed 1000 bytes
        if (chunkSize > 1000)
        {
            chunkSize = 1000;
            Console.WriteLine("Warning: Chunk size reduced to 1000 bytes maximum");
        }

        List<FileChunk> chunks = new();
        string fileId = Tools.Tools.GetNewId(userService);

        byte[] buffer = new byte[chunkSize];
        using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read);

        int bytesRead;
        int chunkNumber = 0;
        int totalChunks = (int)Math.Ceiling((double)fs.Length / chunkSize);

        while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
        {
            byte[] chunkData = new byte[bytesRead];
            Array.Copy(buffer, chunkData, bytesRead);

            var chunk = new FileChunk(
                userService.GetSelfUser().UserName,
                DateTime.Now,
                fileId,
                chunkNumber,
                chunkData
            );
            chunks.Add(chunk);
            chunkNumber++;
        }
        return chunks;
    }

    public async void SendFile(string receiverUserName, string filePath)
    {
        var chunks = SplitFile(filePath, receiverUserName, 800).ToList(); // Using smaller chunks to account for overhead
        double size = new FileInfo(filePath).Length;
        var start = new FileStart(
            userService.GetSelfUser().UserName,
            DateTime.Now,
            Path.GetFileName(filePath),
            size,
            chunks.First().FileId,
            chunks.Count);
        Confirmations.Add(start.FileId, []);
        foreach (var chunk in chunks)
        {
            Confirmations[start.FileId].Add(chunk.ChunkNumber, false);
        }
        ConfirmingStarts.Add(start.FileId, false);
        Console.WriteLine("Confirming Starting");
        Task sendAndWait = Task.Run(async () =>
        {
            Console.WriteLine("Async method");
            System.Console.WriteLine(!ConfirmingStarts[start.FileId]);
            while (!ConfirmingStarts[start.FileId])
            {
                Console.WriteLine("in while");
                byte[] frame = protocolService.CreateFrameToSend(userService.GetUserByName(receiverUserName), start, false);
                Console.WriteLine("Frame has been created");
                await networkService.SendFrameAsync(frame);
                System.Console.WriteLine($"Starting sending fileStart from {start.UserName}");
                await Task.Delay(1000);
            }
        });
        await sendAndWait;
        System.Console.WriteLine("FileStart sended");
        foreach (var chunk in chunks)
        {
            Task sendAndWaitChunk = Task.Run(async () =>
            {
                while (!Confirmations[start.FileId][chunk.ChunkNumber])
                {
                    byte[] frame = protocolService.CreateFrameToSend(userService.GetUserByName(receiverUserName), chunk, false);
                    await networkService.SendFrameAsync(frame);
                    System.Console.WriteLine($"Message sended with ID {chunk.ChunkNumber}");
                    await Task.Delay(3000);
                }
            });
            await sendAndWaitChunk;
        }
    }

    public void OnFileStartAckFrameReceived(FileStartAck fileStartAck)
    {
        ConfirmingStarts[fileStartAck.FileId] = true;
    }

    private void OnFileChunkAckFrameReceived(FileChunkAck fileAck)
    {
        if (Confirmations.ContainsKey(fileAck.FileID) && Confirmations[fileAck.FileID].ContainsKey(fileAck.ChunkNumber))
        {
            Confirmations[fileAck.FileID][fileAck.ChunkNumber] = true;
        }
    }

    private async void OnFileChunkFrameReceived(FileChunk fileChunk)
    {
        await SendChunkConfirmation(fileChunk);
        if (FileChunks.ContainsKey(fileChunk.FileId))
        {
            if (!FileChunks[fileChunk.FileId].ContainsKey(fileChunk.ChunkNumber))
            {
                FileChunks[fileChunk.FileId].Add(fileChunk.ChunkNumber, fileChunk);
                Task task = SendChunkConfirmation(fileChunk);
                await task;
            }

            int cant = FileStarts[fileChunk.FileId].TotalChunks;
            if (cant == FileChunks[fileChunk.FileId].Count)
            {
                var currentUser = Environment.GetEnvironmentVariable("SUDO_USER");
                System.Console.WriteLine($"Detected user: {currentUser}");

                string downloadPath = Path.Combine("/home", currentUser, "Downloads", "LinkChatDownloads");
                System.Console.WriteLine($"Using downloads directory: {downloadPath}");

                if (!Directory.Exists(downloadPath))
                {
                    Directory.CreateDirectory(downloadPath);
                    System.Console.WriteLine($"Created Downloads directory at: {downloadPath}");
                }
                string fileName = FileStarts[fileChunk.FileId].FileName;
                string filePath = Path.Combine(downloadPath, fileName);
                if (!Directory.Exists(downloadPath))
                {
                    throw new DirectoryNotFoundException($"Failed to create directory: {downloadPath}");
                }

                int counter = 1;
                while (System.IO.File.Exists(filePath))
                {
                    string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    string extension = Path.GetExtension(fileName);
                    filePath = Path.Combine(downloadPath, $"{nameWithoutExt}({counter}){extension}");
                    counter++;
                }

                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var orderedChunks = FileChunks[fileChunk.FileId]
                        .OrderBy(chunk => chunk.Key)
                        .ToList();

                    System.Console.WriteLine($"Chunks ordered, about to write {orderedChunks.Count} chunks");

                    foreach (var pair in orderedChunks)
                    {
                        var chunk = pair.Value;
                        System.Console.WriteLine($"Writing chunk {chunk.ChunkNumber}, size: {chunk.Data.Length} bytes");
                        fs.Write(chunk.Data, 0, chunk.Data.Length);
                        fs.Flush();
                    }
                }

                var fileStart = FileStarts[fileChunk.FileId];
                double fileSize = new FileInfo(filePath).Length / 1024.0;
                var file = new Models.File(
                    fileStart.UserName,
                    DateTime.Now,
                    fileChunk.FileId,
                    filePath,
                    fileSize,
                    fileName
                );
                System.Console.WriteLine($"File saved successfully at: {filePath}");
                // Agregar a la lista y notificar
                Files.Add(fileChunk.FileId, file);
                System.Console.WriteLine("Invoke called");
                FileFrameReceived?.Invoke(file);
                // Limpiar los diccionarios temporales
                FileChunks.Remove(fileChunk.FileId);
                FileStarts.Remove(fileChunk.FileId);
            }
        }
    }

    private async void OnFileStartFrameReceived(FileStart fileStart)
    {
        if (!FileChunks.ContainsKey(fileStart.FileId))
        {
            FileStarts.Add(fileStart.FileId, fileStart);
            FileChunks.Add(fileStart.FileId, []);
            Task task = SendStartConfirmation(fileStart);
            await task;
        }
        System.Console.WriteLine($"Recibiendo {fileStart.FileId} mediante {fileStart.TotalChunks} chunks");
    }
    public async Task SendChunkConfirmation(FileChunk fileChunk)
    {
        FileChunkAck fileChunkAck = new FileChunkAck(fileChunk.UserName, DateTime.Now, fileChunk.FileId, fileChunk.ChunkNumber);
        byte[] frame = protocolService.CreateFrameToSend(userService.GetUserByName(fileChunk.UserName), fileChunkAck, false);
        System.Console.WriteLine($"Confirmation sended to fileChunk with ID {fileChunk.FileId}");
        await networkService.SendFrameAsync(frame);
    }
    public async Task SendStartConfirmation(FileStart fileStart)
    {
        FileStartAck fileStartAck = new FileStartAck(fileStart.UserName, DateTime.Now, fileStart.FileId);
        byte[] frame = protocolService.CreateFrameToSend(userService.GetUserByName(fileStart.UserName), fileStartAck, false);
        System.Console.WriteLine($"Confirmation sended to fileStart with ID {fileStart.FileId}");
        await networkService.SendFrameAsync(frame);
    }

    public Models.File GetFileById(string messageId)
    {
        if (Files.ContainsKey(messageId))
        {
            return Files[messageId];
        }
        throw new Exception($"File with Id {messageId} not found");
    }

}