using LinkChat.Core.Services;
using LinkChat.Core.Models;
using LinkChat.Core.Tools;
using System.Threading.Tasks;
using System.Drawing;
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
        int chunkSize = 1024) // 1 KB
    {
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
        var chunks = SplitFile(filePath, receiverUserName, 64 * 1024).ToList();
        double size = new FileInfo(filePath).Length / 1024;
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
        Console.WriteLine("FileStart ack received");
    }

    private void OnFileChunkAckFrameReceived(FileChunkAck fileAck)
    {
        if (Confirmations.ContainsKey(fileAck.FileID) && Confirmations[fileAck.FileID].ContainsKey(fileAck.ChunkNumber))
        {
            Confirmations[fileAck.FileID][fileAck.ChunkNumber] = true;
            System.Console.WriteLine($"Filechunk {fileAck.ChunkNumber} received");
        }
    }

    private async void OnFileChunkFrameReceived(FileChunk fileChunk)
    {
        if (FileChunks.ContainsKey(fileChunk.FileId))
        {
            if (!FileChunks[fileChunk.FileId].ContainsKey(fileChunk.ChunkNumber))
            {
                FileChunks[fileChunk.FileId].Add(fileChunk.ChunkNumber, fileChunk);
                Task task = SendChunkConfirmation(fileChunk);
                await task;
                System.Console.WriteLine($"Receiving chunck {fileChunk.ChunkNumber}");
            }

            int cant = FileStarts[fileChunk.FileId].TotalChunks;
            if (cant == FileChunks[fileChunk.FileId].Count)
            {
                string downloadPath = "/home/" + Environment.UserName + "/Downloads";

                string fileName = FileStarts[fileChunk.FileId].FileName;
                string filePath = Path.Combine(downloadPath, fileName);

                int counter = 1;
                while (System.IO.File.Exists(filePath))
                {
                    string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    string extension = Path.GetExtension(fileName);
                    filePath = Path.Combine(downloadPath, $"{nameWithoutExt}({counter}){extension}");
                    counter++;
                }
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    var orderedChunks = FileChunks[fileChunk.FileId]
                        .OrderBy(chunk => chunk.Key)
                        .Select(chunk => chunk.Value);

                    foreach (var chunk in orderedChunks)
                    {
                        fs.Write(chunk.Data, 0, chunk.Data.Length);
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
        FileStartAck fileStartAck = new FileStartAck(fileChunk.UserName, DateTime.Now, fileChunk.FileId);
        byte[] frame = protocolService.CreateFrameToSend(userService.GetUserByName(fileChunk.UserName), fileStartAck, false);
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