namespace LinkChat.Infrastructure.Linux.Native.Structs
{
    using System.Runtime.InteropServices;

    internal static class NativeStructs
    {
        //Struct ifreq (for ioctl)
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct ifreq
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string ifr_name; // Interface name. IFNAMSIZ is 16 on Linux.
            public int ifr_ifindex; // This is where the kernel will return the index
        }

        // Struct sockaddr_ll (for bind, sendto, recvfrom)
        [StructLayout(LayoutKind.Sequential)]
        public struct sockaddr_ll
        {
            public ushort sll_family;   //Always AF_PACKET
            public ushort sll_protocol; // Frame protocol (your EtherType)
            public int sll_ifindex;  // Interface index
            public ushort sll_hatype;   // Hardware type
            public byte sll_pkttype;  // Package type
            public byte sll_halen;    // address length (6 for MAC)

            // MAC address (6 bytes) + padding (2 bytes)
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] sll_addr;
        }
    }
}