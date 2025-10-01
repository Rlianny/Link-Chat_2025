namespace LinkChat.Infrastructure.Linux.Native.Methods
{
    using System.Runtime.InteropServices;
    using static LinkChat.Infrastructure.Linux.Native.Structs.NativeStructs;

    internal static partial class NativeMethods
    {

        /// <summary>
        /// Create Raw Socket
        /// </summary>
        /// <param name="domain">Specifies the family of communication protocols. For raw Ethernet-level sockets, you should use AF_PACKET.</param>
        /// <param name="type">Defines the communication semantics. To receive complete frames, use SOCK_RAW.</param>
        /// <param name="protocol">Indicates the specific protocol.</param>
        /// <returns>Socket descriptor (integer) or -1 on error.</returns>
        [DllImport("libc", SetLastError = true)]
        public static extern int socket(int domain, int type, int protocol);

        /// <summary>
        /// Associates a socket with a specific network interface address.
        /// </summary>
        /// <param name="sockfd">The socket descriptor returned by the socket function.</param>
        /// <param name="sockaddr_ll">Pointer to a sockaddr_ll structure containing the link-level address information.</param>
        /// <param name="addrlen">Size in bytes of the steering structure (sockaddr_ll) that is passed</param>
        /// <returns>0 for success, -1 for failure.</returns>
        [DllImport("libc", SetLastError = true)]
        private static extern int bind(int sockfd, ref sockaddr_ll addr, int addrlen);

        /// <summary>
        /// Used to manipulate underlying device parameters
        /// </summary>
        /// <param name="fd">The socket descriptor returned by the socket function.</param>
        /// <param name="request">Operation code that specifies the action to be performed.</param>
        /// <param name="ifr">Pointer to an ifreq structure used to pass information to or from the controller. Its contents depend on the request code used.</param>
        /// <returns>0 for success, -1 for failure.</returns>
        [DllImport("libc", SetLastError = true)]
        private static extern int ioctl(int fd, uint request, ref ifreq ifr);

        /// <summary>
        /// It is used to receive data through the raw socket
        /// </summary>
        /// <param name="sockfd">The socket descriptor returned by the socket function.</param>
        /// <param name="buf">Pointer to the buffer where the received data will be stored.</param>
        /// <param name="len">Maximum buffer length.</param>
        /// <param name="flags">Operation modifiers; 0 for normal operation.</param>
        /// <param name="sockaddr_ll">Structure to be filled with the sender's address.</param>
        /// <param name="addrlen">Variable indicating the size of the src_addr structure.</param>
        /// <returns>Number of bytes received, or -1 on error.</returns>
        [DllImport("libc", SetLastError = true)]
        private static extern long recvfrom(int sockfd, byte[] buf, int len, int flags, ref sockaddr_ll saddr, ref int addrlen);

        /// <summary>
        /// It is used to send data through the raw socket.
        /// </summary>
        /// <param name="sockfd">The socket descriptor returned by the socket function.</param>
        /// <param name="buf">Pointer to the buffer where the received data will be stored.</param>
        /// <param name="len">Maximum buffer length.</param>
        /// <param name="flags">Operation modifiers; 0 for normal operation.</param>
        /// <param name="sockaddr_ll">Structure to be filled with the sender's address.</param>
        /// <param name="addrlen">Variable indicating the size of the src_addr structure.</param>
        /// <returns>Number of bytes sent, or -1 on error.</returns>
        [DllImport("libc", SetLastError = true)]
        private static extern int sendto(int sockfd, byte[] buf, int len, int flags, ref sockaddr_ll addr, int addrlen);

        /// <summary>
        /// Closes the socket descriptor when it is no longer needed.
        /// </summary>
        /// <param name="fd">The socket descriptor returned by the socket function.</param>
        /// <returns>0 for success, -1 for failure.</returns>
        [DllImport("libc", SetLastError = true)]
        private static extern int close(int fd);

        // VERY IMPORTANT HELPER FUNCTION: htons (Host to Network Short)
        // Networks use a different byte order (Big Endian) than most CPUs (Little Endian). This function performs the conversion.
        public static ushort htons(ushort value)
        {
            if (!BitConverter.IsLittleEndian)
            {
                return value;
            }
            return (ushort)((value << 8) | (value >> 8));
        }
    }
}