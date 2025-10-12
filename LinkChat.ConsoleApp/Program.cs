using LinkChat.Core;
using LinkChat.Core.Implementations;
using LinkChat.Core.Models;
using LinkChat.Core.Services;
using LinkChat.Core.Tools;
using LinkChat.Infrastructure;

public class Program
{
    private static Dictionary<string, byte[]> MacAddresses = new Dictionary<string, byte[]>
    {
        { "Lianny", [0x90, 0x61, 0xAE, 0x72, 0x6A, 0xD7]},
        { "Kevin", [0x14, 0x13, 0x33, 0x54, 0x79, 0xFF] }
    };
    public static void Main()
    {

        string? myUserName = Environment.GetEnvironmentVariable("LINKCHAT_USERNAME");

        // 2. Si no existe, pídelo interactivamente (para uso manual).
        if (string.IsNullOrEmpty(myUserName))
        {
            Console.WriteLine("Introduce tu nombre de usuario:");
            Console.Out.Flush();
            myUserName = Console.ReadLine();
        }
        else
        {
            Console.WriteLine($"Iniciando como usuario '{myUserName}' (desde variable de entorno).");
        }
        string interfaceName = NetworkInterfaceSelector.GetBestNetworkInterfaceName();

        INetworkService networkService = new LinuxNetworkService(interfaceName);
        IProtocolService protocolService = new ProtocolService(networkService);
        IUserService userService = new UserService(protocolService, networkService, myUserName);
        IFileTransferService fileTransferService = new FileTransferService(protocolService, networkService, userService);
        IMessagingService messagingService = new MessagingService(protocolService, fileTransferService, userService, networkService);
        networkService.StartListening();
        System.Console.WriteLine("Terminando inicialización...");
        Console.Out.Flush();


        while (true)
        {
            List<User> users = userService.GetAvailableUsers();
            Console.WriteLine("Usuarios disponibles:");

            foreach (User user in users)
            {
                Console.WriteLine($"- {user.UserName}");
            }
            Console.WriteLine("Escriba el nombre del receptor del mensaje");
            Console.Out.Flush();
            string receiver = Console.ReadLine();
            System.Console.WriteLine("Escriba el mensaje a enviar a ese usuario");
            Console.Out.Flush();
            string message = Console.ReadLine();

            //messagingService.SendTextMessage(receiver, message);
            messagingService.SendTextMessage(receiver, message);

        }
    }
}
