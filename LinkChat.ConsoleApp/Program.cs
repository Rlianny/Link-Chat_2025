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
using System.Threading.Tasks;

public class Program
{
    private static IMessagingService _messagingService;
    private static IUserService _userService;
    private static IFileTransferService _fileTransferService;
    private static bool _newMessageAvailable = false;
    private static Queue<TextMessage> _receivedMessages = new Queue<TextMessage>();

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
            
            // Suscribirse a eventos de mensajes
            _messagingService.TextMessageExchanged += OnMessageReceived;
            
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

    private static void OnMessageReceived(TextMessage message)
    {
        lock (_receivedMessages)
        {
            _receivedMessages.Enqueue(message);
            _newMessageAvailable = true;
        }
    }

    private static void RunMainMenu()
    {
        bool exit = false;
        
        while (!exit)
        {
            Console.Clear();
            PrintHeader("LINK CHAT - MENÚ PRINCIPAL");
            
            // Mostrar notificación si hay mensajes nuevos
            if (_newMessageAvailable)
            {
                PrintColoredMessage("📬 ¡Tienes mensajes nuevos!", ConsoleColor.Magenta);
                Console.WriteLine();
            }
            
            Console.WriteLine("1️⃣  Ver usuarios conectados");
            Console.WriteLine("2️⃣  Iniciar/continuar chat");
            Console.WriteLine("3️⃣  Ver mensajes recibidos");
            Console.WriteLine("4️⃣  Enviar archivo");
            Console.WriteLine("5️⃣  Actualizar estado");
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
                    StartChatSession();
                    break;
                case "3":
                    ShowReceivedMessages();
                    break;
                case "4":
                    SendFile();
                    break;
                case "5":
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
    
    private static void StartChatSession()
    {
        Console.Clear();
        PrintHeader("INICIAR CHAT");
        
        List<User> users = _userService.GetAvailableUsers();
        
        if (users.Count == 0)
        {
            PrintColoredMessage("⚠️ No hay usuarios disponibles para chatear.", ConsoleColor.Red);
            Console.WriteLine();
            PrintColoredMessage("Presiona cualquier tecla para volver al menú principal...", ConsoleColor.Yellow);
            Console.ReadKey();
            return;
        }
        
        PrintColoredMessage("Selecciona un usuario para chatear:", ConsoleColor.Cyan);
        Console.WriteLine();
        
        for (int i = 0; i < users.Count; i++)
        {
            string statusIcon = GetStatusIcon(users[i].Status);
            Console.WriteLine($"  {i + 1}. {statusIcon} {users[i].UserName}");
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
        StartChat(selectedUser);
    }
    
    private static void StartChat(User recipient)
    {
        bool chatActive = true;
        DateTime lastCheck = DateTime.Now;
        List<TextMessage> chatHistory = new List<TextMessage>();
        
        Console.Clear();
        PrintHeader($"CHAT CON {recipient.UserName}");
        PrintColoredMessage("Escribe tus mensajes. Comandos disponibles:", ConsoleColor.Cyan);
        PrintColoredMessage("  /salir - Volver al menú principal", ConsoleColor.Yellow);
        PrintColoredMessage("  /limpiar - Limpiar historial de la conversación", ConsoleColor.Yellow);
        Console.WriteLine();
        
        while (chatActive)
        {
            // Comprobar si hay mensajes nuevos del usuario con el que estamos chateando
            CheckForNewMessages(recipient.UserName, chatHistory);
            
            // Mostrar prompt de escritura
            Console.Write("> ");
            string message = Console.ReadLine();
            
            // Procesar comandos o enviar mensaje
            if (string.IsNullOrWhiteSpace(message))
            {
                continue;
            }
            else if (message.Equals("/salir", StringComparison.OrdinalIgnoreCase))
            {
                chatActive = false;
            }
            else if (message.Equals("/limpiar", StringComparison.OrdinalIgnoreCase))
            {
                Console.Clear();
                PrintHeader($"CHAT CON {recipient.UserName}");
                chatHistory.Clear();
                PrintColoredMessage("Historial de chat limpiado.", ConsoleColor.Yellow);
            }
            else
            {
                try
                {
                    _messagingService.SendTextMessage(recipient.UserName, message);

                    // Añadir mensaje enviado al historial
                    TextMessage sentMessage = new TextMessage(_userService.GetSelfUser().UserName, DateTime.Now, Tools.GetNewId(_userService), message);
                    
                    chatHistory.Add(sentMessage);
                    
                    // Mostrar mensaje enviado
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Tú: {message}");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    PrintColoredMessage($"❌ Error al enviar mensaje: {ex.Message}", ConsoleColor.Red);
                }
            }
        }
    }
    
    private static void CheckForNewMessages(string fromUser, List<TextMessage> chatHistory)
    {
        lock (_receivedMessages)
        {
            if (_receivedMessages.Count > 0)
            {
                Queue<TextMessage> remainingMessages = new Queue<TextMessage>();
                
                while (_receivedMessages.Count > 0)
                {
                    TextMessage msg = _receivedMessages.Dequeue();
                    
                    if (msg.UserName == fromUser)
                    {
                        // Mostrar y añadir al historial mensajes del usuario actual
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"[{msg.TimeStamp:HH:mm:ss}] {msg.UserName}: {msg.Content}");
                        Console.ResetColor();
                        chatHistory.Add(msg);
                    }
                    else
                    {
                        // Mantener mensajes de otros usuarios en la cola
                        remainingMessages.Enqueue(msg);
                    }
                }
                
                // Restaurar mensajes de otros usuarios a la cola principal
                foreach (var msg in remainingMessages)
                {
                    _receivedMessages.Enqueue(msg);
                }
                
                _newMessageAvailable = _receivedMessages.Count > 0;
            }
        }
    }
    
    private static void ShowReceivedMessages()
    {
        Console.Clear();
        PrintHeader("MENSAJES RECIBIDOS");
        
        lock (_receivedMessages)
        {
            if (_receivedMessages.Count == 0)
            {
                PrintColoredMessage("No hay mensajes nuevos.", ConsoleColor.Yellow);
            }
            else
            {
                Dictionary<string, List<TextMessage>> messagesByUser = new Dictionary<string, List<TextMessage>>();
                
                // Agrupar mensajes por usuario
                foreach (var message in _receivedMessages)
                {
                    if (!messagesByUser.ContainsKey(message.UserName))
                    {
                        messagesByUser[message.UserName] = new List<TextMessage>();
                    }
                    messagesByUser[message.UserName].Add(message);
                }
                
                // Mostrar mensajes agrupados
                foreach (var kvp in messagesByUser)
                {
                    PrintColoredMessage($"De {kvp.Key} ({kvp.Value.Count} mensajes):", ConsoleColor.Cyan);
                    
                    foreach (var message in kvp.Value)
                    {
                        Console.WriteLine($"  [{message.TimeStamp:HH:mm:ss}] {message.Content}");
                    }
                    
                    Console.WriteLine();
                    PrintColoredMessage($"¿Quieres chatear con {kvp.Key}? (s/n): ", ConsoleColor.Yellow);
                    
                    if (Console.ReadLine().Trim().ToLower() == "s")
                    {
                        // Buscar el usuario en la lista de usuarios disponibles
                        User chatUser = _userService.GetAvailableUsers().FirstOrDefault(u => u.UserName == kvp.Key);
                        
                        if (chatUser != null)
                        {
                            // Eliminar los mensajes procesados
                            RemoveProcessedMessages(kvp.Key);
                            StartChat(chatUser);
                            return;  // Salir después de terminar el chat
                        }
                        else
                        {
                            PrintColoredMessage($"⚠️ El usuario {kvp.Key} ya no está disponible.", ConsoleColor.Red);
                        }
                    }
                }
                
                // Actualizar estado de notificación
                _newMessageAvailable = _receivedMessages.Count > 0;
            }
        }
        
        Console.WriteLine();
        PrintColoredMessage("Presiona cualquier tecla para volver al menú principal...", ConsoleColor.Yellow);
        Console.ReadKey();
    }
    
    private static void RemoveProcessedMessages(string userName)
    {
        lock (_receivedMessages)
        {
            Queue<TextMessage> remainingMessages = new Queue<TextMessage>();
            
            while (_receivedMessages.Count > 0)
            {
                TextMessage msg = _receivedMessages.Dequeue();
                if (msg.UserName != userName)
                {
                    remainingMessages.Enqueue(msg);
                }
            }
            
            // Restaurar los mensajes que quedan
            foreach (var msg in remainingMessages)
            {
                _receivedMessages.Enqueue(msg);
            }
            
            _newMessageAvailable = _receivedMessages.Count > 0;
        }
    }
    
    private static void SendFile()
    {
        Console.Clear();
        PrintHeader("ENVIAR ARCHIVO");
        
        PrintColoredMessage("⚙️ Funcionalidad en desarrollo. Disponible próximamente.", ConsoleColor.Magenta);
        
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
        Console.WriteLine("3. Ocupado");
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
            Status.Offline => "🔴",
            _ => "⚪"
        };
    }
}