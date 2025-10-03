using LinkChat.Core.Services;

namespace LinkChat.Core.Implementations;

public class FileTransferService : IFileTransferService
{
    public event Action<Models.File> FileFrameReceived;

    public FileTransferService()
    {
    
    }
    public Models.File GetFileById(int message)
    {
        throw new NotImplementedException();
    }

    public void SendFile(string receiverUserName, string filePath)
    {
        throw new NotImplementedException();
    }
}