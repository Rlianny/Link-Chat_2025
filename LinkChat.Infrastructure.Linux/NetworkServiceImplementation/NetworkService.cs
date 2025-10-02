using LinkChat.Core.Models;
using LinkChat.Core.Services;
using LinkChat.Infrastructure.Linux.Native.Methods;
using LinkChat.Infrastructure.Linux.Native.Structs;

namespace LinkChat.Infrastructure
{
    public sealed class LinuxNetworkService : INetworkService, IDisposable
    {
        private readonly string interfaceName;
        private int socketFd = -1; // Socket File Descriptor, -1 means it is not initialized
        private int interfaceIndex;
        private byte[] localMacAddress = new byte[6];
        private readonly object sendLock = new object();

        public event Action<byte[]>? FrameReceived;

        public void SendMessage(User user, Message message)
        {
            throw new NotImplementedException();
        }

        public Task SendFrameAsync(byte[] frame)
        {
            throw new NotImplementedException();
        }

        public void StartListening()
        {
            throw new NotImplementedException();
        }

        private void ListenLoop()
        {
            byte[] buffer = new byte[1518]; // we are considering a MTU = 1500

            while (socketFd != -1)
            {
                try
                {

                    NativeStructs.sockaddr_ll tempAddr = new NativeStructs.sockaddr_ll();
                    int tempAddrLen = 0;
                    int receivedBytes = (int)NativeMethods.recvfrom(socketFd, buffer, buffer.Length, 0, ref tempAddr, ref tempAddrLen);

                    if (receivedBytes > 0)
                    {
                        byte[] frameData = new byte[receivedBytes];
                        Array.Copy(buffer, 0, frameData, 0, receivedBytes);

                        FrameReceived?.Invoke(frameData);
                    }
                }
                catch (Exception ex)
                {
                    if (socketFd != -1)
                    {
                        Console.WriteLine($"Error in the listening loop: {ex.Message}");
                    }
                }
            }
        }

        public void Dispose()
        {
            if (socketFd != -1)
            {
                NativeMethods.close(socketFd);
                socketFd = -1;
            }
        }
    }
}