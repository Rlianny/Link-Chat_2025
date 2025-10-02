using LinkChat.Core.Models;
using LinkChat.Core.Services;
using LinkChat.Infrastructure.Linux.Native.Methods;
using LinkChat.Infrastructure.Linux.Native.Structs;
using LinkChat.Infrastructure.Linux.Native.Constants;
using System.Runtime.InteropServices;

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

        public LinuxNetworkService()
        {
            
        }

        public Task SendFrameAsync(byte[] frame)
        {
            if (socketFd == -1)
                throw new InvalidOperationException("The socket is not initialized.");

            var destAddr = new NativeStructs.sockaddr_ll
            {
                sll_family = NativeConstants.AF_PACKET,
                sll_ifindex = interfaceIndex,
                sll_halen = 6, // MAC Address length
                sll_addr = new byte[8] // The first 6 bytes are the MAC, the rest are zero.
            };

            Array.Copy(frame, 0, destAddr.sll_addr, 0, 6); // The first 6 bytes (destination MAC) are copied to the struct

            lock (sendLock)
            {
                int sentBytes = NativeMethods.sendto(socketFd, frame, frame.Length, 0, ref destAddr, Marshal.SizeOf(destAddr));
                if (sentBytes < 0)
                {
                    var error = Marshal.GetLastWin32Error();
                    throw new Exception($"Error sending package. System error code: {error}");
                }
            }

            return Task.CompletedTask;
        }

        public void StartListening()
        {
            if (socketFd == -1)
                throw new InvalidOperationException("The socket is not initialized.");

            // Start the listening loop in a new thread to avoid blocking the application.
            Task.Run(() => ListenLoop());
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