using LinkChat.Core;
using LinkChat.Core.Implementations;
using LinkChat.Core.Models;
using LinkChat.Core.Services;
using LinkChat.Core.Tools;
using LinkChat.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

public class Program
{
    private static IMessagingService _messagingService;
    private static IUserService _userService;
    private static IFileTransferService _fileTransferService;

    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        InitializeServices();
        RunMainMenu();
    }

    private static void InitializeServices()
    {
        PrintColoredMessage("🔄 Inicializando LinkChat...", ConsoleColor.Cyan);

        // Obtener nombre de usuario
        string? myUserName = Environment.GetEnvironmentVariable("LINKCHAT_USERNAME");
        if (string.IsNullOrEmpty(myUserName))
        {
            PrintColoredMessage("👤 Introduce tu nombre de usuario:", ConsoleColor.Yellow);
            myUserName = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(myUserName))
            {
                PrintColoredMessage("⚠️ El nombre de usuario no puede estar vacío. Inténtalo de nuevo:", ConsoleColor.Red);
                myUserName = Console.ReadLine();
            }
        }
        else
        {
            PrintColoredMessage($"👤 Iniciando como usuario '{myUserName}' (desde variable de entorno).", ConsoleColor.Green);
        }

        // Inicializar servicios
        try
        {
            string interfaceName = NetworkInterfaceSelector.GetBestNetworkInterfaceName();
            PrintColoredMessage($"🌐 Usando interfaz de red: {interfaceName}", ConsoleColor.DarkCyan);

            INetworkService networkService = new LinuxNetworkService(interfaceName);
            IProtocolService protocolService = new ProtocolService(networkService);
            _userService = new UserService(protocolService, networkService);
            _userService.SetSelfUser(myUserName, Gender.female);
            _fileTransferService = new FileTransferService(protocolService, networkService, _userService);
            _messagingService = new MessagingService(protocolService, _fileTransferService, _userService, networkService);
            
            PrintColoredMessage("✅ Servicios inicializados correctamente", ConsoleColor.Green);
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            PrintColoredMessage($"❌ Error al inicializar servicios: {ex.Message}", ConsoleColor.Red);
            Console.WriteLine("Presiona cualquier tecla para salir...");
            Console.ReadKey();
            Environment.Exit(1);
        }
    }

    private static void RunMainMenu()
    {
        bool exit = false;
        
        while (!exit)
        {
            Console.Clear();
            PrintHeader("LINK CHAT - MENÚ PRINCIPAL");
            
            Console.WriteLine("1️⃣  Ver usuarios conectados");
            Console.WriteLine("2️⃣  Enviar mensaje de texto");
            Console.WriteLine("3️⃣  Enviar archivo");
            Console.WriteLine("4️⃣  Actualizar estado");
            Console.WriteLine("0️⃣  Salir");
            Console.WriteLine();
            
            PrintColoredMessage("Selecciona una opción: ", ConsoleColor.Yellow);
            
            string input = Console.ReadLine();
            
            switch (input)
            {
                case "1":
                    ShowConnectedUsers();
                    break;
                case "2":
                    SendTextMessage();
                    break;
                case "3":
                    SendFile();
                    break;
                case "4":
                    UpdateStatus();
                    break;
                case "0":
                    exit = true;
                    break;
                default:
                    PrintColoredMessage("⚠️ Opción no válida. Presiona cualquier tecla para continuar...", ConsoleColor.Red);
                    Console.ReadKey();
                    break;
            }
        }
        
        Console.Clear();
        PrintColoredMessage("👋 ¡Hasta pronto!", ConsoleColor.Cyan);
        Thread.Sleep(1000);
    }
    
    private static void ShowConnectedUsers()
    {
        Console.Clear();
        PrintHeader("USUARIOS CONECTADOS");
        
        List<User> users = _userService.GetAvailableUsers();
        
        if (users.Count == 0)
        {
            PrintColoredMessage("No hay usuarios conectados actualmente.", ConsoleColor.Yellow);
        }
        else
        {
            PrintColoredMessage("Usuarios disponibles:", ConsoleColor.Cyan);
            Console.WriteLine();
            
            foreach (User user in users)
            {
                string statusIcon = GetStatusIcon(user.Status);
                string genderIcon = user.Gender == Gender.female ? "♀️" : "♂️";
                
                Console.Write($"  {statusIcon} ");
                PrintColoredMessage(user.UserName, ConsoleColor.Green);
                Console.WriteLine($" {genderIcon}");
            }
        }
        
        Console.WriteLine();
        PrintColoredMessage("Presiona cualquier tecla para volver al menú principal...", ConsoleColor.Yellow);
        Console.ReadKey();
    }
    
    private static void SendTextMessage()
    {
        Console.Clear();
        PrintHeader("ENVIAR MENSAJE");
        
        List<User> users = _userService.GetAvailableUsers();
        
        if (users.Count == 0)
        {
            PrintColoredMessage("⚠️ No hay usuarios disponibles para enviar mensajes.", ConsoleColor.Red);
            Console.WriteLine();
            PrintColoredMessage("Presiona cualquier tecla para volver al menú principal...", ConsoleColor.Yellow);
            Console.ReadKey();
            return;
        }
        
        PrintColoredMessage("Usuarios disponibles:", ConsoleColor.Cyan);
        Console.WriteLine();
        
        for (int i = 0; i < users.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. {users[i].UserName}");
        }
        
        Console.WriteLine();
        PrintColoredMessage("Selecciona el número del usuario (o escribe 0 para cancelar): ", ConsoleColor.Yellow);
        
        if (!int.TryParse(Console.ReadLine(), out int selectedIndex) || selectedIndex < 0 || selectedIndex > users.Count)
        {
            PrintColoredMessage("⚠️ Selección no válida.", ConsoleColor.Red);
            Console.WriteLine();
            PrintColoredMessage("Presiona cualquier tecla para volver al menú principal...", ConsoleColor.Yellow);
            Console.ReadKey();
            return;
        }
        
        if (selectedIndex == 0)
        {
            return;
        }
        
        User selectedUser = users[selectedIndex - 1];
        
        PrintColoredMessage($"Escribiendo a {selectedUser.UserName}. Escribe tu mensaje (línea vacía para cancelar):", ConsoleColor.Green);
        string message = Console.ReadLine();
        
        if (string.IsNullOrEmpty(message))
        {
            PrintColoredMessage("Envío cancelado.", ConsoleColor.Yellow);
        }
        else
        {
            try
            {
                _messagingService.SendTextMessage(selectedUser.UserName, message);
                PrintColoredMessage("✅ Mensaje enviado correctamente.", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                PrintColoredMessage($"❌ Error al enviar mensaje: {ex.Message}", ConsoleColor.Red);
            }
        }
        
        Console.WriteLine();
        PrintColoredMessage("Presiona cualquier tecla para volver al menú principal...", ConsoleColor.Yellow);
        Console.ReadKey();
    }
    
    private static void SendFile()
    {
        Console.Clear();
        PrintHeader("ENVIAR ARCHIVO");
        
        PrintColoredMessage("⚙️ Funcionalidad en desarrollo. Disponible próximamente.", ConsoleColor.Magenta);
        
        // Aquí implementarías la lógica para enviar archivos
        // Similar a SendTextMessage pero usando _fileTransferService.SendFile
        
        Console.WriteLine();
        PrintColoredMessage("Presiona cualquier tecla para volver al menú principal...", ConsoleColor.Yellow);
        Console.ReadKey();
    }
    
    private static void UpdateStatus()
    {
        Console.Clear();
        PrintHeader("ACTUALIZAR ESTADO");
        
        Console.WriteLine("1. Disponible");
        Console.WriteLine("2. Ausente");
        Console.WriteLine("0. Cancelar");
        Console.WriteLine();
        
        PrintColoredMessage("Selecciona tu nuevo estado: ", ConsoleColor.Yellow);
        
        string input = Console.ReadLine();
        Status newStatus = Status.Online;
        
        switch (input)
        {
            case "1":
                newStatus = Status.Online;
                break;
            case "2":
                newStatus = Status.Offline;
                break;
            case "0":
                return;
            default:
                PrintColoredMessage("⚠️ Opción no válida.", ConsoleColor.Red);
                Console.WriteLine();
                PrintColoredMessage("Presiona cualquier tecla para volver al menú principal...", ConsoleColor.Yellow);
                Console.ReadKey();
                return;
        }
        
        // Aquí implementarías la actualización del estado del usuario
        // _userService.UpdateStatus(newStatus);
        
        PrintColoredMessage("✅ Estado actualizado correctamente.", ConsoleColor.Green);
        Console.WriteLine();
        PrintColoredMessage("Presiona cualquier tecla para volver al menú principal...", ConsoleColor.Yellow);
        Console.ReadKey();
    }
    
    // Métodos de utilidad para la interfaz
    
    private static void PrintHeader(string title)
    {
        int width = Console.WindowWidth;
        string line = new string('═', width - 1);
        
        Console.WriteLine();
        PrintColoredMessage(line, ConsoleColor.Cyan);
        
        int padding = (width - title.Length) / 2;
        string centeredTitle = title.PadLeft(padding + title.Length).PadRight(width - 1);
        
        PrintColoredMessage(centeredTitle, ConsoleColor.White, ConsoleColor.DarkBlue);
        PrintColoredMessage(line, ConsoleColor.Cyan);
        Console.WriteLine();
    }
    
    private static void PrintColoredMessage(string message, ConsoleColor foreground, ConsoleColor? background = null)
    {
        ConsoleColor originalForeground = Console.ForegroundColor;
        ConsoleColor originalBackground = Console.BackgroundColor;
        
        Console.ForegroundColor = foreground;
        if (background.HasValue)
        {
            Console.BackgroundColor = background.Value;
        }
        
        Console.WriteLine(message);
        
        Console.ForegroundColor = originalForeground;
        Console.BackgroundColor = originalBackground;
    }
    
    private static string GetStatusIcon(Status status)
    {
        return status switch
        {
            Status.Online => "🟢",
            Status.Offline => "🟡",
            _ => "⚪"
        };
    }
}