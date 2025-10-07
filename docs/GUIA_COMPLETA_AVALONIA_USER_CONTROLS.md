# Guía Completa para Implementar User Controls en Avalonia

## 1. Conceptos Fundamentales de Avalonia

### 1.1 ¿Qué es un User Control?
Un User Control es un componente reutilizable que encapsula:
- **Vista (XAML)**: La interfaz visual
- **Lógica de UI (Code-behind)**: Manejo de eventos y inicialización
- **Datos (ViewModel)**: La lógica de negocio y estado

### 1.2 Patrón MVVM (Model-View-ViewModel)
```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│     View        │    │   ViewModel      │    │     Model       │
│  (XAML + CS)    │◄──►│  (Lógica + Datos)│    │  (Datos Puros)  │
└─────────────────┘    └──────────────────┘    └─────────────────┘
       │                        │                        │
       │                        │                        │
       └────────────────────────┼────────────────────────┘
                                │
                       ┌──────────────────┐
                       │    Binding       │
                       │  (DataContext)   │
                       └──────────────────┘
```

### 1.3 DataContext y Binding
- **DataContext**: Es el "puente" entre la vista y los datos
- **Binding**: Conecta propiedades del XAML con propiedades del ViewModel
- **Ejemplo**: `Text="{Binding NombreUsuario}"` busca la propiedad `NombreUsuario` en el DataContext

## 2. Pasos para Crear un User Control

### Paso 2.1: Crear el ViewModel

```csharp
// 1. Crear archivo ViewModel
// Ejemplo: ViewModels/MiComponenteViewModel.cs

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LinkChat.Desktop.Avalonia.ViewModels;

public partial class MiComponenteViewModel : ViewModelBase
{
    // 2. Definir propiedades observables
    [ObservableProperty]
    private string _titulo = "Texto por defecto";

    [ObservableProperty]
    private bool _isVisible = true;

    // 3. Definir comandos con RelayCommand
    [RelayCommand]
    private void MiAccion()
    {
        // Lógica del comando
        Titulo = "¡Clic realizado!";
    }

    // 4. Constructor si necesitas inicialización
    public MiComponenteViewModel()
    {
        // Inicialización si es necesaria
    }
}
```

### Paso 2.2: Crear el User Control (XAML)

```xml
<!-- Views/MiComponente.axaml -->

<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:vm="using:LinkChat.Desktop.Avalonia.ViewModels"
             mc:Ignorable="d" d:DesignWidth="400" d:DesignHeight="300"
             x:Class="LinkChat.Desktop.Avalonia.Views.MiComponente"
             x:DataType="vm:MiComponenteViewModel">

    <StackPanel Spacing="10">
        <!-- Ejemplo de TextBlock con binding -->
        <TextBlock Text="{Binding Titulo}"
                   FontSize="16"
                   Foreground="Black"/>

        <!-- Ejemplo de Button con comando -->
        <Button Content="Haz clic"
                Command="{Binding MiAccionCommand}"
                HorizontalAlignment="Center"
                Padding="10,5"/>

        <!-- Ejemplo de control condicional -->
        <Border Background="LightBlue"
                CornerRadius="5"
                Padding="10"
                IsVisible="{Binding IsVisible}">
            <TextBlock Text="Este contenido es condicional"/>
        </Border>
    </StackPanel>
</UserControl>
```

### Paso 2.3: Crear el Code-Behind

```csharp
// Views/MiComponente.axaml.cs

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LinkChat.Desktop.Avalonia.ViewModels;

namespace LinkChat.Desktop.Avalonia.Views;

public partial class MiComponente : UserControl
{
    // 1. Propiedad para recibir datos del padre (opcional)
    public static readonly StyledProperty<MiComponenteViewModel> MiViewModelProperty =
        AvaloniaProperty.Register<MiComponente, MiComponenteViewModel>(nameof(MiViewModel));

    public MiComponenteViewModel MiViewModel
    {
        get => GetValue(MiViewModelProperty);
        set => SetValue(MiViewModelProperty, value);
    }

    // 2. Constructor
    public MiComponente()
    {
        InitializeComponent();
    }

    // 3. Inicialización del componente
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        // Configurar DataContext si tienes propiedad personalizada
        if (MiViewModel != null)
        {
            DataContext = MiViewModel;
        }

        // Escuchar cambios en la propiedad personalizada
        this.PropertyChanged += (s, e) =>
        {
            if (e.Property == MiViewModelProperty)
            {
                DataContext = MiViewModel;
            }
        };
    }
}
```

## 3. Usar el User Control en una Ventana Padre

### Paso 3.1: En el XAML Padre

```xml
<!-- En tu ventana padre (ej: ChatWindow.axaml) -->

<Window xmlns:local="clr-namespace:LinkChat.Desktop.Avalonia.Views"
        xmlns:vm="using:LinkChat.Desktop.Avalonia.ViewModels">

    <StackPanel>
        <!-- Forma 1: Sin propiedad personalizada (DataContext automático) -->
        <local:MiComponente/>

        <!-- Forma 2: Con propiedad personalizada (recomendado) -->
        <local:MiComponente x:Name="MiComponentePersonalizado"
                           MiViewModel="{Binding MiViewModelDesdePadre}"/>

        <!-- Forma 3: Con nombre para acceso programático -->
        <local:MiComponente x:Name="ComponenteAccesible"/>
    </StackPanel>
</Window>
```

### Paso 3.2: En el Code-Behind Padre

```csharp
// ChatWindow.axaml.cs

public partial class ChatWindow : Window
{
    public ChatWindow()
    {
        InitializeComponent();

        // 1. Crear ViewModel para el componente hijo
        var viewModelHijo = new MiComponenteViewModel
        {
            Titulo = "¡Hola desde el padre!",
            IsVisible = true
        };

        // 2. Opción A: Usar FindControl (como hicimos antes)
        var componente = this.FindControl<MiComponente>("MiComponentePersonalizado");
        if (componente != null)
        {
            componente.MiViewModel = viewModelHijo;
        }

        // 3. Opción B: Crear componente completamente en código
        var componenteNuevo = new MiComponente
        {
            MiViewModel = viewModelHijo
        };
        // Agregarlo al contenedor...
    }
}
```

## 4. Errores Comunes y Cómo Evitarlos

### Error 1: DataContext es null
**Síntoma**: Los bindings no funcionan, comandos no se ejecutan

**Causa**: El ViewModel no se asignó correctamente al DataContext

**Solución**:
```csharp
// ✅ CORRECTO
private void InitializeComponent()
{
    AvaloniaXamlLoader.Load(this);

    // Establecer DataContext inmediatamente
    if (MiViewModel != null)
    {
        DataContext = MiViewModel;
    }

    // Escuchar cambios
    this.PropertyChanged += (s, e) =>
    {
        if (e.Property == MiViewModelProperty)
        {
            DataContext = MiViewModel;
        }
    };
}
```

### Error 2: Comandos no se generan
**Síntoma**: `MiComandoCommand` no existe

**Causa**: Falta `[RelayCommand]` o no se instaló CommunityToolkit.Mvvm

**Solución**:
```csharp
// ✅ CORRECTO
[RelayCommand]
private void MiAccion() // ❌ NO pongas "Command" aquí
{
    // Lógica aquí
}

// El atributo genera automáticamente: MiAccionCommand
```

### Error 3: PropertyChanged no se dispara
**Síntoma**: Los cambios en el padre no se reflejan en el hijo

**Causa**: No se configuró correctamente el evento PropertyChanged

**Solución**:
```csharp
// ✅ CORRECTO - En el padre
var componente = this.FindControl<MiComponente>("NombreComponente");
if (componente != null)
{
    componente.MiViewModel = nuevoViewModel; // Esto dispara PropertyChanged
}
```

### Error 4: Referencias circulares
**Síntoma**: Stack overflow, aplicación se cuelga

**Causa**: ViewModel referencia al padre que referencia al ViewModel

**Solución**:
```csharp
// ❌ EVITAR
public class ViewModelHijo
{
    public ChatWindow Padre { get; set; } // ¡Referencia circular!
}

// ✅ CORRECTO
public class ViewModelHijo
{
    public Action<string> NotificarAlPadre { get; set; } // Solo callback
}
```

## 5. Buenas Prácticas

### 5.1 Organización de Archivos
```
ViewModels/
├── ViewModelBase.cs           # Clase base común
├── ChatWindowViewModel.cs     # ViewModel de ventanas principales
├── BubbleMessageViewModel.cs  # ViewModel base para mensajes
└── [Componente]ViewModel.cs   # Un ViewModel por componente

Views/
├── ChatWindow.axaml           # Ventanas principales
├── View[Componente].axaml     # User Controls
└── View[Componente].axaml.cs  # Code-behind
```

### 5.2 Nombres de Comandos
```csharp
// ✅ Buena convención
[RelayCommand]
private void Guardar() // Genera: GuardarCommand

[RelayCommand]
private void CargarDatos() // Genera: CargarDatosCommand

[RelayCommand(CanExecute = nameof(PuedeEjecutar))]
private void Ejecutar() // Con validación
```

### 5.3 Validación de Comandos
```csharp
// ✅ Agregar validación
[RelayCommand(CanExecute = nameof(PuedeGuardar))]
private void Guardar()
{
    // Lógica de guardado
}

public bool PuedeGuardar => !string.IsNullOrEmpty(Texto);
```

## 6. Debugging y Diagnóstico

### 6.1 Logs de Debug
```csharp
// En ViewModel
[RelayCommand]
private void AccionDebug()
{
    Console.WriteLine($"DataContext: {DataContext?.GetType().Name}");
    Console.WriteLine($"Propiedad: {MiPropiedad}");
    Console.WriteLine($"Comando ejecutado correctamente");
}
```

### 6.2 Verificar Bindings en Tiempo de Ejecución
```csharp
// En el code-behind del componente
private void InitializeComponent()
{
    AvaloniaXamlLoader.Load(this);

    if (MiViewModel != null)
    {
        DataContext = MiViewModel;
        Console.WriteLine($"✅ DataContext establecido: {DataContext.GetType().Name}");
    }
    else
    {
        Console.WriteLine($"❌ MiViewModel es null");
    }
}
```

### 6.3 Usar el Output Window de Visual Studio
- Ve a: **Vista > Salida**
- Selecciona: **Mostrar salida de: Depurar**
- Busca mensajes de error de binding

## 7. Ejemplo Completo: Componente de Mensaje

Basado en tu componente que acabamos de arreglar:

```csharp
// ViewModels/MessageBubbleViewModel.cs
public partial class MessageBubbleViewModel : BubbleMessageViewModel
{
    [ObservableProperty]
    private string _messageText;

    [ObservableProperty]
    private string _senderName;

    public MessageBubbleViewModel(string text, string sender) : base(null)
    {
        MessageText = text;
        SenderName = sender;
    }
}
```

```xml
<!-- Views/ViewMessageBubble.axaml -->
<UserControl xmlns:vm="using:LinkChat.Desktop.Avalonia.ViewModels"
             x:Class="LinkChat.Desktop.Avalonia.Views.ViewMessageBubble"
             x:DataType="vm:MessageBubbleViewModel">

    <Border Background="{Binding MessageType, Converter={...}}"
            CornerRadius="10"
            Padding="15">
        <StackPanel>
            <TextBlock Text="{Binding SenderName}"
                       FontWeight="Bold"/>
            <TextBlock Text="{Binding MessageText}"
                       TextWrapping="Wrap"/>
            <Button Content="Reaccionar"
                    Command="{Binding ToggleReactionMenuCommand}"
                    HorizontalAlignment="Right"/>
        </StackPanel>
    </Border>
</UserControl>
```

## 8. Próximos Pasos para Tu Proyecto

1. **Identifica cada componente** que necesitas (lista de usuarios, mensajes, etc.)
2. **Crea el ViewModel** para cada uno siguiendo el patrón mostrado
3. **Implementa el XAML** con los bindings necesarios
4. **Configura el code-behind** con las propiedades personalizadas
5. **Integra en el padre** usando `FindControl` o bindings
6. **Prueba cada componente** individualmente antes de integrarlo

## 9. Recursos Adicionales

- **Documentación oficial**: https://docs.avaloniaui.net/
- **Community Toolkit**: https://github.com/CommunityToolkit/dotnet
- **Ejemplos**: Busca "Avalonia MVVM" en GitHub

## 10. Checklist para Cada Nuevo Componente

- [ ] ¿Creé el ViewModel con [ObservableProperty]?
- [ ] ¿Usé [RelayCommand] para los métodos?
- [ ] ¿Configuré el DataContext correctamente?
- [ ] ¿Agregué logs de debug para verificar funcionamiento?
- [ ] ¿Probé el componente independientemente?
- [ ] ¿Configuré correctamente la comunicación padre-hijo?
- [ ] ¿Revisé errores comunes de esta guía?

¡Sigue esta guía paso a paso y podrás implementar todos tus componentes sin problemas! Cada error que cometas te enseñará algo nuevo sobre Avalonia.