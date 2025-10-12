using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using LinkChat.Core.Models;
using LinkChat.Core.Services;
using LinkChat.Infrastructure.Linux.Native.Methods;
using LinkChat.Infrastructure.Linux.Native.Structs;
using LinkChat.Infrastructure.Linux.Native.Constants;
using LinkChat.Core.Tools;

namespace LinkChat.Infrastructure
{
    public sealed class LinuxNetworkService : INetworkService, IDisposable
    {
        private const int MTU = 1500; // Ethernet standard MTU
        private const int ETHERNET_HEADER_SIZE = 14; // Size of Ethernet frame header
        private const int MAX_PAYLOAD_SIZE = MTU - ETHERNET_HEADER_SIZE - 100; // Leave some room for protocol overhead

        private readonly string interfaceName;
        private int socketFd = -1; // Socket File Descriptor, -1 means it is not initialized
        private int interfaceIndex;
        private readonly object sendLock = new object();
        private bool isRunning = false;
        private readonly object queueLock = new object();
        private readonly PriorityQueue<byte[], int> queue = new PriorityQueue<byte[], int>();

        public event Action<byte[]>? FrameReceived;

        public LinuxNetworkService(string interfaceName)
        {
            Console.WriteLine("The network service is created");
            this.interfaceName = interfaceName;
            CreateSocket();
            GetInterfaceIndex();
            BindSocketToInterface();
            isRunning = true;
            StartSendLoop();
        }

        private void StartSendLoop()
        {
            Task sendTask = Task.Run(async () =>
              {
                  while (isRunning)
                  {
                      byte[]? frame = null;

                      lock (queueLock)
                      {
                          if (queue.Count > 0)
                          {
                              frame = queue.Dequeue();
                             // Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Frame DEQUEUED, remaining={queue.Count}");
                          }
                      }

                      if (frame != null)
                      {
                          try
                          {
                             // Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Sending frame to network...");
                              await SendFrameInternal(frame);
                             // Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Frame SENT successfully");
                          }
                          catch (Exception ex)
                          {
                            //  Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Error sending frame: {ex.Message}");
                          }
                      }
                      else
                      {
                          await Task.Delay(1);
                      }
                  }
              });
         //   Console.WriteLine("Send loop started in background");
        }

        public Task SendFrameAsync(byte[] frame, int priority)
        {
            var timestamp = DateTime.Now;
            lock (queueLock)
            {
                queue.Enqueue(frame, priority);
               // Console.WriteLine($"[{timestamp:HH:mm:ss.fff}] Frame ENQUEUED priority={priority}, queue size={queue.Count}");
            }
            return Task.CompletedTask;
        }

        public Task SendFrameInternal(byte[] frame)
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

            if (frame.Length > MTU)
            {
                throw new InvalidOperationException($"Frame size ({frame.Length} bytes) exceeds MTU ({MTU} bytes). Consider reducing chunk size.");
            }

            lock (sendLock)
            {
                // Use MSG_DONTWAIT flag to force immediate send without kernel buffering
                int sentBytes = NativeMethods.sendto(socketFd, frame, frame.Length, NativeConstants.MSG_DONTWAIT, ref destAddr, Marshal.SizeOf(destAddr));
                if (sentBytes < 0)
                {
                    var error = Marshal.GetLastWin32Error();
                    if (error == 90) // EMSGSIZE
                    {
                        throw new InvalidOperationException($"Frame size ({frame.Length} bytes) is too large for the network interface. Maximum payload size should be {MAX_PAYLOAD_SIZE} bytes.");
                    }
                    throw new Exception($"Error sending package. System error code: {error}");
                }
            }

            return Task.CompletedTask;
        }

        private void CreateSocket()
        {
            ushort protocol = Tools.htons(NativeConstants.LINK_CHAT_ETHER_TYPE);
            socketFd = NativeMethods.socket(NativeConstants.AF_PACKET, NativeConstants.SOCK_RAW, protocol);

            if (socketFd < 0)
            {
                var error = Marshal.GetLastWin32Error();
                throw new Exception($"Error creating socket. System error code: {error}");
            }
               // Aumentar el buffer de recepción del socket
            try
            {
                int receiveBufferSize = 1024 * 1024; // 1 MB
                int result = NativeMethods.setsockopt(socketFd, NativeConstants.SOL_SOCKET, NativeConstants.SO_RCVBUF, 
                    ref receiveBufferSize, sizeof(int));
                
                if (result < 0)
                {
                    Console.WriteLine($"Warning: Could not set socket receive buffer size");
                }
                else
                {
                    Console.WriteLine($"Socket receive buffer set to {receiveBufferSize} bytes");
                }

                // Deshabilitar buffering en envío
                int sendBufferSize = 0; // 0 = sin buffering
                result = NativeMethods.setsockopt(socketFd, NativeConstants.SOL_SOCKET, NativeConstants.SO_SNDBUF, 
                    ref sendBufferSize, sizeof(int));
                
                if (result < 0)
                {
                    Console.WriteLine($"Warning: Could not disable send buffer");
                }
                else
                {
                    Console.WriteLine($"Send buffering disabled for immediate transmission");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Error setting socket buffer: {ex.Message}");
            }
        }
        private void GetInterfaceIndex()
        {
            var ifr = new NativeStructs.ifreq { ifr_name = this.interfaceName };

            if (NativeMethods.ioctl(socketFd, NativeConstants.SIOCGIFINDEX, ref ifr) < 0)
            {
                var error = Marshal.GetLastWin32Error();
                throw new Exception($"Error getting index for interface '{this.interfaceName}'. Code: {error}");
            }

            this.interfaceIndex = ifr.ifr_ifindex;
        }



        private void BindSocketToInterface()
        {
            var addr = new NativeStructs.sockaddr_ll
            {
                sll_family = NativeConstants.AF_PACKET,
                sll_protocol = Tools.htons(NativeConstants.LINK_CHAT_ETHER_TYPE),
                sll_ifindex = this.interfaceIndex
            };

            if (NativeMethods.bind(socketFd, ref addr, Marshal.SizeOf(addr)) < 0)
            {
                var error = Marshal.GetLastWin32Error();
                throw new Exception($"Error binding socket to interface. Code: {error}");
            }
        }

        public void StartListening()
        {
            if (socketFd == -1)
                throw new InvalidOperationException("The socket is not initialized.");

            // Start the listening loop in a new thread with higher priority 
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
                      //  Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Frame RECEIVED from network, size={receivedBytes} bytes");

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