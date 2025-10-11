namespace LinkChat.Infrastructure.Linux.Native.Constants
{
    using LinkChat.Core.Tools;
    internal static class NativeConstants
    {
        //const for socket()
        public const int AF_PACKET = 17; // Specifies the "family" of the network. The value 17 specifies that you want to work directly with link layer (Layer 2) frames, without involving IP.
        public const int SOCK_RAW = 3; // Defines the socket type. A value of 3 indicates that you want to handle the entire packet, including the frame header.
        public const ushort LINK_CHAT_ETHER_TYPE = Tools.ETHER_TYPE; //customized ether type

        //const for ioctl()
        public const int SIOCGIFINDEX = 0x8933; //It is the operation code that stands for "Get Interface Index"

        //const for setsockopt()
        public const int SOL_SOCKET = 1; // Socket level options
        public const int SO_RCVBUF = 8; // Receive buffer size
        public const int SO_SNDBUF = 7; // Send buffer size

        //const for sendto() flags
        public const int MSG_DONTWAIT = 0x40; // Non-blocking operation, send immediately
    }
}