using System.Reflection.Metadata;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinkChat.Core.Implementations;
using LinkChat.Core.Services;
using LinkChat.Infrastructure;

namespace LinkChat.Desktop.Avalonia.ViewModels;
using LinkChat.Core.Models;
using System;

public partial class ChatWindowViewModel: ViewModelBase
{
    public AppManager AppManager; 
    private User _currentRecieverUser;
    
    [ObservableProperty]
    private string _messageInPlaceHolder = string.Empty;

    private ReceivedBubbleTextMessageViewModel _message1;
    private ReceivedBubbleTextMessageViewModel _message2;
    private SendedBubbleTextMessageViewModel _message3;
    private ReceivedBubbleTextMessageViewModel _message4;
    private SendedBubbleTextMessageViewModel _message5;
    private ReceivedBubbleTextMessageViewModel _message6;
    private ReceivedBubbleFileMessageViewModel _receivedBubbleFileMessage;
    
    private bool _broadcast = false;
    [ObservableProperty] private Bitmap _broadcastIcon;
    
    public ReceivedBubbleTextMessageViewModel Message1 
    { 
        get => _message1;
        set => SetProperty(ref _message1, value);
    }

    public ReceivedBubbleTextMessageViewModel Message2
    {
        get => _message2;
        set => SetProperty(ref _message2, value);
    }

    public SendedBubbleTextMessageViewModel Message3
    {
        get => _message3;
        set => SetProperty(ref  _message3, value);
    }
    
    public ReceivedBubbleTextMessageViewModel Message4
    {
        get => _message4;
        set => SetProperty(ref _message4, value);
    }
    
    public SendedBubbleTextMessageViewModel Message5
    {
        get => _message5;
        set => SetProperty(ref  _message5, value);
    }
    
    public ReceivedBubbleTextMessageViewModel Message6
    {
        get => _message6;
        set => SetProperty(ref _message6, value);
    }

    public ReceivedBubbleFileMessageViewModel FileMessage
    {
        get => _receivedBubbleFileMessage;
        set => SetProperty(ref _receivedBubbleFileMessage, value);
    }

    public ChatWindowViewModel()
    {
        INetworkService networkService = new FakeNetworkService();
        IProtocolService protocolService = new ProtocolService(networkService);
        IUserService userService = new UserService(protocolService, networkService, "Lianny");
        IFileTransferService fileTransferService = new FileTransferService(protocolService, networkService, userService);
        IMessagingService messagingService = new MessagingService(protocolService, fileTransferService, userService, networkService);
        AppManager _appManager = new AppManager(networkService, protocolService, userService, fileTransferService,  messagingService );
        _broadcastIcon = ImageHelper.LoadFromResource(new Uri("avares://LinkChat.Desktop.Avalonia/Assets/Images/BroadcastDisabledBold.png"));
        
        TextMessage textMessage = new TextMessage("Bianca", DateTime.Now, "123", "Hola Diana!");
        _message1 = new ReceivedBubbleTextMessageViewModel(textMessage, _appManager);
        
        TextMessage textMessage2 = new TextMessage("Bianca", DateTime.Now, "456", "Todo bien?");
        _message2 = new ReceivedBubbleTextMessageViewModel(textMessage2,  _appManager);
        
        TextMessage  textMessage3 = new TextMessage(" Diana", DateTime.Now, "123", "Hey, todo bien, y tú?");
        _message3 = new SendedBubbleTextMessageViewModel(textMessage3, _appManager);
        
        TextMessage textMessage4 = new TextMessage("Bianca", DateTime.Now, "123", "Bien, te escribía para invitarte al cine esta tarde. Iremos con Alicia, te apuntas?");
        _message4 = new ReceivedBubbleTextMessageViewModel(textMessage4, _appManager);
        
        TextMessage textMessage5 = new TextMessage("Diana", DateTime.Now, "123", "Por supuesto! Ahí estaré");
        _message5 = new SendedBubbleTextMessageViewModel(textMessage5, _appManager);
        
        TextMessage textMessage6 = new TextMessage("Bianca", DateTime.Now, "123", "Perfecto, te envío la cartelera. Nos vemos!");
        _message6 = new ReceivedBubbleTextMessageViewModel(textMessage6, _appManager);
        
        File file = new File("Bianca", DateTime.Now, "123", "path", 3.5, "Cartelera");
        _receivedBubbleFileMessage = new ReceivedBubbleFileMessageViewModel(file,  _appManager); 
    }

    [RelayCommand]
    public void BroadacasteToggle()
    {
        _broadcast =  !_broadcast;
        if (_broadcast)
        {
            BroadcastIcon = ImageHelper.LoadFromResource(new Uri("avares://LinkChat.Desktop.Avalonia/Assets/Images/BroadcastActiveBold.png"));
        }
        else BroadcastIcon = ImageHelper.LoadFromResource(new Uri("avares://LinkChat.Desktop.Avalonia/Assets/Images/BroadcastDisabledBold.png"));
    }

    [RelayCommand]
    public async void SendFileButton()
    {
        var storageProvider = GlobalSingletonHelper.StorageProvider;
        
        var fileResults = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Selecciona un archivo para enviar",
            AllowMultiple = false,
            FileTypeFilter = null 
        });
        
        if (fileResults != null && fileResults.Count > 0)
        {
            var selectedFile = fileResults[0];
            //Console.WriteLine(selectedFile.Path.AbsolutePath.ToString());
            await AppManager.SendFileMessage(selectedFile.Path.AbsolutePath.ToString(), _currentRecieverUser.UserName);
        }
    }

    [RelayCommand]
    public async void SendTextMessageButton()
    {
        AppManager.SendTextMessage(MessageInPlaceHolder,  _currentRecieverUser.UserName);
        Console.WriteLine($"The message {MessageInPlaceHolder} will be sent");
        MessageInPlaceHolder = string.Empty;
    }

    private void TextBox_KeyDown(object sender, KeyEventArgs key)
    {
        bool isLetter = (key.Key >= Key.A && key.Key <= Key.Z);
        
        bool isNumber = (key.Key >= Key.D0 && key.Key <= Key.D9) || 
                        (key.Key >= Key.NumPad0 && key.Key <= Key.NumPad9);
        
        bool isSpace = key.Key == Key.Space;

        if (isLetter || isNumber || isSpace)
        {
            Console.WriteLine($"The key {key.Key} will be sent");
            AppManager.SendUserStatusTyping();
        }
        
    }
}