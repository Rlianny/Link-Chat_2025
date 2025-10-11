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
    private const int WINDOW_SIZE = 32;
    Dictionary<string, Models.File> Files = [];
    Dictionary<string, Dictionary<int, FileChunk>> FileChunks = [];
    Dictionary<string, FileStart> FileStarts = [];
    Dictionary<string, Dictionary<int, bool>> Confirmations = [];
    Dictionary<string, bool> ConfirmingStarts = [];
    private IProtocolService protocolService;
    private INetworkService networkService;
    private IUserService userService;

    public event Action<Models.File> FileFrameReceived;
    public event Action<Models.File> FileSended;

    public FileTransferService(IProtocolService protocolService, INetworkService networkService, IUserService userService)
    {
        this.protocolService = protocolService;
        this.networkService = networkService;
        this.userService = userService;
        protocolService.FileStartFrameReceived += OnFileStartFrameReceived;
        protocolService.FileChunkFrameReceived += OnFileChunkFrameReceived;
        protocolService.FileChunkAckFrameReceived += OnFileChunkAckFrameReceived;
        protocolService.FileStartAckFrameReceived += OnFileStartAckFrameReceived;

        Console.WriteLine("The file transfer service is created");
    }
    public List<FileChunk> SplitFile(
        string filePath,
        string userName,
        int chunkSize = 800)
    {
        if (chunkSize > 800)
        {
            chunkSize = 800;
        }

        List<FileChunk> chunks = new();
        string fileId = Tools.Tools.GetNewId(userService);
        Console.WriteLine($"[SplitFile] Starting - fileId={fileId} file={filePath} chunkSize={chunkSize}");

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
        Console.WriteLine($"[SplitFile] Complete - fileId={fileId} totalChunks={chunks.Count} expectedChunks={totalChunks}");
        return chunks;
    }

    public async void SendFile(string receiverUserName, string filePath)
    {
        Console.WriteLine($"[SendFile] START - to={receiverUserName} file={filePath}");
        User receiverUser = userService.GetUserByName(receiverUserName);
        var chunks = SplitFile(filePath, receiverUserName, 800).ToList();
        double size = new FileInfo(filePath).Length;
        Models.File file = new Models.File(userService.GetSelfUser().UserName, DateTime.Now, chunks.First().FileId, filePath, Math.Round(size / (1024 * 1024), 2), Path.GetFileName(filePath));
        Console.WriteLine($"[SendFile] Created File object - fileId={file.MessageId} totalChunks={chunks.Count} size={size}");
        FileSended?.Invoke(file);
        var start = new FileStart(
            file.UserName,
            DateTime.Now,
            file.Name,
            size,
            file.MessageId,
            chunks.Count);
        Confirmations.Add(start.FileId, []);
        foreach (var chunk in chunks)
        {
            Confirmations[start.FileId].Add(chunk.ChunkNumber, false);
        }
        ConfirmingStarts.Add(start.FileId, false);
        Console.WriteLine($"[SendFile] Sending FileStart - fileId={start.FileId} fileName={start.FileName} totalChunks={start.TotalChunks}");
        Task sendAndWait = Task.Run(async () =>
        {
            int attempts = 0;
            while (!ConfirmingStarts[start.FileId])
            {
                attempts++;
                Console.WriteLine($"[SendFile] FileStart attempt #{attempts} - fileId={start.FileId}");
                byte[] frame = protocolService.CreateFrameToSend(receiverUser, start, false);
                await networkService.SendFrameAsync(frame, 4);
                await Task.Delay(100);
            }
            Console.WriteLine($"[SendFile] FileStart ACK received after {attempts} attempts - fileId={start.FileId}");
        });
        await sendAndWait;
        Console.WriteLine($"[SendFile] Starting chunk transmission - fileId={start.FileId} totalChunks={chunks.Count} windowSize={WINDOW_SIZE}");
        for (int i = 0; i < chunks.Count; i += WINDOW_SIZE)
        {
            int windowEnd = Math.Min(i + WINDOW_SIZE, chunks.Count);
            Console.WriteLine($"[SendFile] Window [{i}-{windowEnd - 1}] - fileId={start.FileId}");

            for (int j = i; j < windowEnd; j++)
            {
                byte[] frame = protocolService.CreateFrameToSend(receiverUser, chunks[j], false);
                await networkService.SendFrameAsync(frame, 5);
                Console.WriteLine($"[SendFile] Sent chunk {j}/{chunks.Count} - fileId={start.FileId} bytes={chunks[j].Data.Length}");
            }

            await Task.Delay(50);

            while (true)
            {
                int pending = 0;
                for (int j = i; j < windowEnd; j++)
                {
                    if (!Confirmations[start.FileId][j])
                    {
                        byte[] frame = protocolService.CreateFrameToSend(receiverUser, chunks[j], false);
                        await networkService.SendFrameAsync(frame, 5);
                        pending++;
                        Console.WriteLine($"[SendFile] Resending chunk {j} (pending={pending}) - fileId={start.FileId}");
                    }
                }

                if (pending == 0)
                {
                    Console.WriteLine($"[SendFile] Window [{i}-{windowEnd - 1}] complete - fileId={start.FileId}");
                    break;
                }

                int delay = pending > 10 ? 50 : 20;
                await Task.Delay(delay);
            }
        }
        Console.WriteLine($"[SendFile] COMPLETE - fileId={start.FileId} allChunksSent={chunks.Count}");
    }
    public void OnFileStartAckFrameReceived(FileStartAck fileStartAck)
    {
        Console.WriteLine($"[OnFileStartAckReceived] fileId={fileStartAck.FileId} from={fileStartAck.UserName}");
        ConfirmingStarts[fileStartAck.FileId] = true;
    }

    private void OnFileChunkAckFrameReceived(FileChunkAck fileAck)
    {
        Console.WriteLine($"[OnFileChunkAckReceived] fileId={fileAck.FileID} chunk={fileAck.ChunkNumber} from={fileAck.UserName}");
        if (Confirmations.ContainsKey(fileAck.FileID) && Confirmations[fileAck.FileID].ContainsKey(fileAck.ChunkNumber))
        {
            Confirmations[fileAck.FileID][fileAck.ChunkNumber] = true;
            Console.WriteLine($"[OnFileChunkAckReceived] Confirmed chunk {fileAck.ChunkNumber} - fileId={fileAck.FileID}");
        }
        else
        {
            Console.WriteLine($"[OnFileChunkAckReceived] ERROR: Unknown fileId or chunk - fileId={fileAck.FileID} chunk={fileAck.ChunkNumber}");
        }
    }

    private string GetUserDownloadsPath()
    {
        string homeDir = Environment.GetEnvironmentVariable("SUDO_USER") is { } sudoUser && !string.IsNullOrEmpty(sudoUser)
            ? Path.Combine("/home", sudoUser)
            : Environment.GetEnvironmentVariable("HOME") ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        if (string.IsNullOrEmpty(homeDir))
        {
            return "/tmp/LinkChatDownloads";
        }

        string userDirsPath = Path.Combine(homeDir, ".config", "user-dirs.dirs");
        string xdgDownloadDir = null;

        if (System.IO.File.Exists(userDirsPath))
        {
            try
            {
                var lines = System.IO.File.ReadAllLines(userDirsPath);
                foreach (var line in lines)
                {
                    if (line.StartsWith("XDG_DOWNLOAD_DIR="))
                    {
                        xdgDownloadDir = line.Substring("XDG_DOWNLOAD_DIR=".Length)
                                             .Trim('"')
                                             .Replace("$HOME", homeDir);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error reading {userDirsPath}: {ex.Message}");
            }
        }

        string downloadsFolder = xdgDownloadDir ?? Path.Combine(homeDir, "Downloads");

        return Path.Combine(downloadsFolder, "LinkChatDownloads");
    }




    private async void OnFileChunkFrameReceived(FileChunk fileChunk)
    {
        Console.WriteLine($"[OnFileChunkReceived] fileId={fileChunk.FileId} chunk={fileChunk.ChunkNumber} bytes={fileChunk.Data.Length} from={fileChunk.UserName}");
        await SendChunkConfirmation(fileChunk);
        userService.UpdateLastSeen(fileChunk.UserName);
        if (FileChunks.ContainsKey(fileChunk.FileId))
        {
            if (!FileChunks[fileChunk.FileId].ContainsKey(fileChunk.ChunkNumber))
            {
                FileChunks[fileChunk.FileId].Add(fileChunk.ChunkNumber, fileChunk);
                Console.WriteLine($"[OnFileChunkReceived] Added chunk {fileChunk.ChunkNumber} - fileId={fileChunk.FileId} totalReceived={FileChunks[fileChunk.FileId].Count}");
                Task task = SendChunkConfirmation(fileChunk);
                await task;
            }
            else
            {
                Console.WriteLine($"[OnFileChunkReceived] Duplicate chunk {fileChunk.ChunkNumber} ignored - fileId={fileChunk.FileId}");
            }

            int cant = FileStarts[fileChunk.FileId].TotalChunks;
            Console.WriteLine($"[OnFileChunkReceived] Progress - fileId={fileChunk.FileId} received={FileChunks[fileChunk.FileId].Count}/{cant}");
            if (cant == FileChunks[fileChunk.FileId].Count)
            {
                Console.WriteLine($"[OnFileChunkReceived] ALL CHUNKS RECEIVED - fileId={fileChunk.FileId} starting file assembly");

                string downloadPath = GetUserDownloadsPath();
                System.Console.WriteLine($"Final download path: {downloadPath}");

                Directory.CreateDirectory(downloadPath);

                string fileName = FileStarts[fileChunk.FileId].FileName;
                System.Console.WriteLine(fileName);
                string filePath = Path.Combine(downloadPath, fileName);
                System.Console.WriteLine(filePath);

                int counter = 1;
                while (System.IO.File.Exists(filePath))
                {
                    string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    string extension = Path.GetExtension(fileName);
                    filePath = Path.Combine(downloadPath, $"{nameWithoutExt}({counter}){extension}");
                    counter++;
                }
                System.Console.WriteLine(filePath);

                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    long totalBytesWritten = 0;
                    for (int i = 0; i < FileChunks[fileChunk.FileId].Count; i++)
                    {
                        var chunk = FileChunks[fileChunk.FileId][i];
                        fs.Write(chunk.Data, 0, chunk.Data.Length);
                        fs.Flush();
                        totalBytesWritten += chunk.Data.Length;
                    }
                    Console.WriteLine($"[OnFileChunkReceived] File written - path={filePath} totalBytes={totalBytesWritten}");
                }

                var fileStart = FileStarts[fileChunk.FileId];
                System.Console.WriteLine(filePath);
                double fileSize = new FileInfo(filePath).Length / (1024 * 1024);
                var file = new Models.File(
                    fileStart.UserName,
                    DateTime.Now,
                    fileChunk.FileId,
                    filePath,
                    fileSize,
                    fileName
                );
                Files.Add(fileChunk.FileId, file);
                Console.WriteLine($"[OnFileChunkReceived] File COMPLETE - fileId={fileChunk.FileId} path={filePath} sizeKB={fileSize}");
                FileFrameReceived?.Invoke(file);
                FileChunks.Remove(fileChunk.FileId);
                FileStarts.Remove(fileChunk.FileId);
            }
        }
    }

    private async void OnFileStartFrameReceived(FileStart fileStart)
    {
        Console.WriteLine($"[OnFileStartReceived] fileId={fileStart.FileId} fileName={fileStart.FileName} from={fileStart.UserName} totalChunks={fileStart.TotalChunks} size={fileStart.FileSize}");
        if (!FileChunks.ContainsKey(fileStart.FileId))
        {
            FileStarts.Add(fileStart.FileId, fileStart);
            FileChunks.Add(fileStart.FileId, []);
            Console.WriteLine($"[OnFileStartReceived] Registered fileId={fileStart.FileId} - sending ACK");
            Task task = SendStartConfirmation(fileStart);
            userService.UpdateLastSeen(fileStart.UserName);
            await task;
        }
        else
        {
            Console.WriteLine($"[OnFileStartReceived] ERROR: Duplicate FileStart - fileId={fileStart.FileId}");
        }
    }
    public async Task SendChunkConfirmation(FileChunk fileChunk)
    {
        Console.WriteLine($"[SendChunkConfirmation] Sending ACK - fileId={fileChunk.FileId} chunk={fileChunk.ChunkNumber} to={fileChunk.UserName}");
        FileChunkAck fileChunkAck = new FileChunkAck(fileChunk.UserName, DateTime.Now, fileChunk.FileId, fileChunk.ChunkNumber);
        byte[] frame = protocolService.CreateFrameToSend(userService.GetUserByName(fileChunk.UserName), fileChunkAck, false);
        await networkService.SendFrameAsync(frame, 1);
    }
    public async Task SendStartConfirmation(FileStart fileStart)
    {
        Console.WriteLine($"[SendStartConfirmation] Sending ACK - fileId={fileStart.FileId} to={fileStart.UserName}");
        FileStartAck fileStartAck = new FileStartAck(fileStart.UserName, DateTime.Now, fileStart.FileId);
        byte[] frame = protocolService.CreateFrameToSend(userService.GetUserByName(fileStart.UserName), fileStartAck, false);
        await networkService.SendFrameAsync(frame, 1);
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