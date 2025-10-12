using System;
using Avalonia;
using Avalonia.Controls;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinkChat.Core.Models;
using LinkChat.Desktop.Avalonia.Views;

namespace LinkChat.Desktop.Avalonia.ViewModels;
public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private bool _isMaleSelected = false;

    [ObservableProperty]
    private bool _isFemaleSelected = false;

    [ObservableProperty]
    private bool _isSignInEnabled = false;
    
    partial void OnUsernameChanged(string value)
    {
        ValidateForm();
    }
    
    partial void OnIsMaleSelectedChanged(bool value)
    {
        if (value && IsFemaleSelected)
        {
            IsFemaleSelected = false;
        }
        ValidateForm();
    }
    
    partial void OnIsFemaleSelectedChanged(bool value)
    {
        if (value && IsMaleSelected)
        {
            IsMaleSelected = false;
        }
        ValidateForm();
    }
    
    private void ValidateForm()
    {
        // Enable sign in button only when username is not empty and exactly one gender is selected
        bool hasValidUsername = !string.IsNullOrWhiteSpace(Username) && Username.Length <= 15;
        bool hasGender = IsMaleSelected || IsFemaleSelected;
        IsSignInEnabled = hasValidUsername && hasGender;
    }
    
    [RelayCommand]
    private async Task SignIn()
    {
        if (!IsSignInEnabled)
            return;

        // Determine the selected gender
        Gender gender = IsMaleSelected ? Gender.male : Gender.female;

        ChatWindowViewModel chatWindowViewModel = GlobalSingletonHelper.ChatWindowViewModel;

        // Create the user
        GlobalSingletonHelper.AppManager.SetSelfUserData(Username, gender);
        User user = GlobalSingletonHelper.AppManager.GetCurrentSelfUser();

        // Set user in GlobalSingletonHelper
        GlobalSingletonHelper.SetMyUser(user);
        
        // Switch to ChatWindow using the static reference to desktop
        var desktop = App.DesktopLifetime;
        if (desktop != null)
        {
            var chatWindow = new ChatWindow
            {
                DataContext = chatWindowViewModel
            };
            chatWindow.Show();

            // Cierra la ventana actual (MainWindow)
            desktop.MainWindow?.Close();

            // Asigna la nueva ventana como principal
            desktop.MainWindow = chatWindow;
        }
    }
}