using System;
using System.Threading.Tasks;
using LinkChat.Core.Services;

namespace LinkChat.Desktop.Avalonia.ViewModels;

public class FakeNetworkService : INetworkService
{
    public Task SendFrameAsync(byte[] frame)
    {
        throw new NotImplementedException();
    }

    public Task SendFrameAsync(byte[] frame, int priority)
    {
        throw new NotImplementedException();
    }

    public void StartListening()
    {
       
    }

    public event Action<byte[]>? FrameReceived;
}