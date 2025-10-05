using System;
using System.Linq;
using LinkChat.Core.Services;
using System.Net.NetworkInformation;

namespace LinkChat.Core.Tools
{
    public static class Tools
    {
        public const ushort ETHER_TYPE = 0x88B5;
        public static byte[] ParseMacAddress(string macAddress)
        {
            return macAddress.Split(':', '-')
                             .Select(b => Convert.ToByte(b, 16))
                             .ToArray();
        }
        public static string GetNewId(IUserService userService)
        {
            var timestamp = DateTime.UtcNow.Ticks; // 100ns precision
            var random = Random.Shared.Next(10000);
            string userName = userService.GetSelfUser().UserName;
            return $"{userName}_{timestamp}_{random:0000}";
        }

        public static byte[] GetLocalMacAddress()
        {
            // Find the first operational, non-loopback network interface
            NetworkInterface? activeInterface = NetworkInterface
                .GetAllNetworkInterfaces()
                .FirstOrDefault(nic => nic.OperationalStatus == OperationalStatus.Up &&
                                        nic.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            if (activeInterface != null)
            {
                // Get the physical address (MAC)
                PhysicalAddress direccionMac = activeInterface.GetPhysicalAddress();

                // Convert directly to byte[]
                byte[] macBytes = direccionMac.GetAddressBytes();
                return macBytes;
            }

            throw new InvalidOperationException("A valid network interface could not be found.");
        }

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
