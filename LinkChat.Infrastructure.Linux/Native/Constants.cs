namespace LinkChat
{
    internal static class NativeConstants
    {
        //const for socket()
        const int AF_PACKET = 17; // Specifies the "family" of the network. The value 17 specifies that you want to work directly with link layer (Layer 2) frames, without involving IP.
        const int SOCK_RAW = 3; // Defines the socket type. A value of 3 indicates that you want to handle the entire packet, including the frame header.
        const ushort LINK_CHAT_ETHER_TYPE = 0x88B5; //customized ether type

        //const for ioctl()
        const int SIOCGIFINDEX = 0x8933; //It is the operation code that stands for "Get Interface Index"
    }
}