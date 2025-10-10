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
        int chunkSize = 800)
    {
        if (chunkSize > 800)
        {
            chunkSize = 800;
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
        User receiverUser = userService.GetUserByName(receiverUserName);
        var chunks = SplitFile(filePath, receiverUserName, 800).ToList();
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
        Task sendAndWait = Task.Run(async () =>
        {
            System.Console.WriteLine(!ConfirmingStarts[start.FileId]);
            while (!ConfirmingStarts[start.FileId])
            {
                byte[] frame = protocolService.CreateFrameToSend(receiverUser, start, false);
                await networkService.SendFrameAsync(frame, 4);
            }
        });
        await sendAndWait;
        for (int i = 0; i < chunks.Count; i += WINDOW_SIZE)
        {
            int windowEnd = Math.Min(i + WINDOW_SIZE, chunks.Count);

            for (int j = i; j < windowEnd; j++)
            {
                byte[] frame = protocolService.CreateFrameToSend(receiverUser, chunks[j], false);
                await networkService.SendFrameAsync(frame, 5);
            }
            while (true)
            {
                int pending = 0;
                for (int j = i; j < windowEnd; j++)
                {
                    if (!Confirmations[start.FileId].ContainsKey(j))
                    {
                        byte[] frame = protocolService.CreateFrameToSend(receiverUser, chunks[j], false);
                        await networkService.SendFrameAsync(frame, 5);
                        pending++;
                    }
                }

                if (pending == 0)
                    break;

                int delay = pending > 10 ? 50 : 20;
                await Task.Delay(50);
            }
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
        await SendChunkConfirmation(fileChunk);
        userService.UpdateLastSeen(fileChunk.UserName);
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

                string downloadPath = GetUserDownloadsPath();
                System.Console.WriteLine($"Final download path: {downloadPath}");

                Directory.CreateDirectory(downloadPath);

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

                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    for (int i = 0; i < FileChunks[fileChunk.FileId].Count; i++)
                    {
                        var chunk = FileChunks[fileChunk.FileId][i];
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
                Files.Add(fileChunk.FileId, file);
                FileFrameReceived?.Invoke(file);
                FileChunks.Remove(fileChunk.FileId);
                FileStarts.Remove(fileChunk.FileId);
            }
        }
    }

    private async void OnFileStartFrameReceived(FileStart fileStart)
    {
        System.Console.WriteLine($"File start received for file {fileStart.FileName} with ID {fileStart.FileId} from user {fileStart.UserName}");
        if (!FileChunks.ContainsKey(fileStart.FileId))
        {
            FileStarts.Add(fileStart.FileId, fileStart);
            FileChunks.Add(fileStart.FileId, []);
            Task task = SendStartConfirmation(fileStart);
            userService.UpdateLastSeen(fileStart.UserName);
            await task;
        }
    }
    public async Task SendChunkConfirmation(FileChunk fileChunk)
    {
        FileChunkAck fileChunkAck = new FileChunkAck(fileChunk.UserName, DateTime.Now, fileChunk.FileId, fileChunk.ChunkNumber);
        byte[] frame = protocolService.CreateFrameToSend(userService.GetUserByName(fileChunk.UserName), fileChunkAck, false);
        await networkService.SendFrameAsync(frame, 1);
    }
    public async Task SendStartConfirmation(FileStart fileStart)
    {
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