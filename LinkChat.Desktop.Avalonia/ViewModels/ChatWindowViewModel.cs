using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection.Metadata;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinkChat.Core.Implementations;
using LinkChat.Core.Services;
using LinkChat.Core.Tools;
using LinkChat.Infrastructure;

namespace LinkChat.Desktop.Avalonia.ViewModels;

using LinkChat.Core.Models;
using System;

public partial class ChatWindowViewModel : ViewModelBase
{
    public AppManager AppManager { get; }
    private User? _currentRecieverUser;

    private bool _broadcast = false;
    [ObservableProperty]
    private Bitmap? _broadcastIcon;

    [ObservableProperty]
    private IStorageProvider? _storageProvider;

    public ChatWindowViewModel(AppManager appManager)
    {
        AppManager = appManager;
        AppManager.NewUserDetected += OnNewUserDetected;
        AppManager.UserPruned += OnUserPruned;
    }

    private void OnNewUserDetected(object? sender, User newUser)
    {
        AvailableUsers.Add(new AvailableUserViewModel(newUser, AppManager));
    }

    private void OnUserPruned(object? sender, User user)
    {
        var userToRemove = AvailableUsers.FirstOrDefault(u => u.User.UserName == user.UserName);
        if (userToRemove != null)
        {
            AvailableUsers.Remove(userToRemove);
        }
    }

    [ObservableProperty]
    private string _messageInPlaceHolder = string.Empty;

    public ObservableCollection<BubbleMessageViewModel> CurrentChatHistory { get; set; } = new ObservableCollection<BubbleMessageViewModel>();
    public ObservableCollection<AvailableUserViewModel> AvailableUsers { get; set; } = new ObservableCollection<AvailableUserViewModel>();

    public void AddNewChatMessage(ChatMessage newMessage)
    {
        if (newMessage is TextMessage textMessage)
        {
            if (textMessage.UserName == AppManager.GetCurrentSelfUser().UserName)
                CurrentChatHistory.Add(new SendedBubbleTextMessageViewModel(textMessage, AppManager));
            else CurrentChatHistory.Add(new ReceivedBubbleTextMessageViewModel(textMessage, AppManager));
        }
        else if (newMessage is File file)
        {
            if (file.UserName == AppManager.GetCurrentSelfUser().UserName)
                CurrentChatHistory.Add(new SendedBubbleFileMessageViewModel(file, AppManager));
            else CurrentChatHistory.Add(new ReceivedBubbleFileMessageViewModel(file, AppManager));
        }

    }

    public void AddNewAvailableUser(User user)
    {
        AvailableUsers.Add(new AvailableUserViewModel(user, AppManager));
    }

    public ChatWindowViewModel()
    {
        GlobalSingletonHelper.ChatWindowViewModel = this;
        GlobalSingletonHelper.SetUserName("Lianny");
        
        string interfaceName = NetworkInterfaceSelector.GetBestNetworkInterfaceName();
        INetworkService networkService = new LinuxNetworkService(interfaceName);
        //INetworkService networkService = new FakeNetworkService();
        IProtocolService protocolService = new ProtocolService(networkService);
        IUserService userService = new UserService(protocolService, networkService, "Lianny");
        IFileTransferService fileTransferService = new FileTransferService(protocolService, networkService, userService);
        IMessagingService messagingService = new MessagingService(protocolService, fileTransferService, userService, networkService);
        AppManager = new AppManager(networkService, protocolService, userService, fileTransferService, messagingService);
        _broadcastIcon = ImageHelper.LoadFromResource(new Uri("avares://LinkChat.Desktop.Avalonia/Assets/Images/BroadcastDisabledBold.png"));

        networkService.StartListening();
        //userService.UpdateUsersStatuses();

        AppManager.NewUserDetected += OnNewUserDetected;
        AppManager.TextMessageExchanged += OnTextMessageExchanged;

    }

    private void OnTextMessageExchanged(object? sender, ChatMessage e)
    {
        Console.WriteLine("Se ha detectado un nuevo mensaje en el frontend");
        if (e.UserName == _currentRecieverUser.UserName || e.UserName == AppManager.GetCurrentSelfUser().UserName)
        {
            AddNewChatMessage(e);
        }
    }

    [RelayCommand]
    public void BroadcastToggle()
    {
        _broadcast = !_broadcast;
        BroadcastIcon = ImageHelper.LoadFromResource(new Uri(_broadcast
            ? "avares://LinkChat.Desktop.Avalonia/Assets/Images/BroadcastActiveBold.png"
            : "avares://LinkChat.Desktop.Avalonia/Assets/Images/BroadcastDisabledBold.png"));
    }

    [RelayCommand]
    public async Task SendFileButton()
    {
        if (StorageProvider == null) return;
        var storageProvider = StorageProvider;
        var fileResults = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select a file"
        });

        IStorageFile? selectedFile = fileResults.FirstOrDefault();
        if (selectedFile != null && _currentRecieverUser != null)
        {
            await AppManager.SendFileMessage(selectedFile.Path.AbsolutePath.ToString(), _currentRecieverUser.UserName);
        }
    }

    [RelayCommand]
    public async Task SendTextMessageButton()
    {
        if (MessageInPlaceHolder == string.Empty) return;
        if (_broadcast)
        {
            AppManager.SendBroadcast(MessageInPlaceHolder);
        }
        else if (_currentRecieverUser != null)
        {
            AppManager.SendTextMessage(MessageInPlaceHolder, _currentRecieverUser.UserName);
            
        }
        MessageInPlaceHolder =  string.Empty;
    }


    public void ReloadChat(string newReceiverName)
    {
        _currentRecieverUser = AppManager.GetUserByName(newReceiverName);
        if (_currentRecieverUser != null)
        {
            List<ChatMessage> chatMessages = AppManager.GetChatHistory(_currentRecieverUser.UserName);
            foreach (var chatMessage in chatMessages)
                AddNewChatMessage(chatMessage);
        }
    }

    public async Task TextBox_KeyDown(object sender, KeyEventArgs key)
    {
        bool isLetter = (key.Key >= Key.A && key.Key <= Key.Z);

        bool isNumber = (key.Key >= Key.D0 && key.Key <= Key.D9) ||
                        (key.Key >= Key.NumPad0 && key.Key <= Key.NumPad9);

        bool isSpace = key.Key == Key.Space;

        if (isLetter || isNumber || isSpace)
        {
            Console.WriteLine($"The key {key.Key} will be sent");
            await AppManager.SendUserStatusTyping();
        }
    }
}