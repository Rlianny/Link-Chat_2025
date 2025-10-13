> Proyecto de Programación
> Facultad de Matemática y Computación - Universidad de La Habana.
> Curso 2025
![Have you delete the image?](.//LinkChat.Desktop.Avalonia/Assets/Images/Banner2.png)

# Link-Chat

## Overview
LinkChat is a peer-to-peer chat application that operates directly at the link layer (Layer 2) of the network stack. It enables direct communication between devices on the same local network without requiring internet connectivity or traditional client-server infrastructure.

## Features
* Text messaging between users on the same network
* File sharing capabilities
* Message reactions with emojis (like, dislike, heart, etc.)
* User presence detection
* Message delivery confirmation
* AES encrypted communications
* Cross-platform desktop application (using Avalonia UI)
* Console application for headless environments

## System Requirements
* .NET 9.0 Runtime
* Linux operating system (for raw socket support)
* Administrator/root privileges (required for link layer access)
* Network interface that supports raw packet manipulation

##Installation

### From Source
You can skip this step if you are running the desktop application.
1.  **Clone the repository**
    ```sh
    git clone https://github.com/Rlianny/Link-Chat_2025.git
    cd Link-Chat_2025
    ```
2.  **Build application**
    ```sh
    dotnet build
    ```
### Using Docker
```bash
# Build the Docker image
docker build -t linkchat .

# Run the console application in a container
docker run --network host --cap-add=NET_RAW --cap-add=NET_ADMIN -e LINKCHAT_USERNAME=YourName linkchat
```

## Usage

### Avalonia
1. Run the application with administrator privileges
```sh
sudo ./LinkChatDesktop
```
2. Enter your username and select your gender
3. Start chatting with other users on the network


### Console Application
1. Run with administrator privileges
```sh
cd LinkChat.ConsoleApp
sudo dotnet run --project LinkChat.ConsoleApp.csproj
```
2. Follow the prompts to enter your username
3. The application will automatically discover other users on the network

## Architecture

LinkChat consists of several key components:

- **LinkChat.Core**: Core business logic, protocol definition, and models
- **LinkChat.Infrastructure.Linux**: Linux-specific network implementation using raw sockets
- **LinkChat.Desktop.Avalonia**: Cross-platform desktop UI using Avalonia
- **LinkChat.ConsoleApp**: Command-line interface implementation

The application uses a custom Ethernet protocol (EtherType 0x88B5) for communication and implements AES encryption for message security.

## Technical Details
- Raw socket implementation for direct link layer communication
- AES encryption for secure messaging
- Custom protocol with message acknowledgment system
- File transfer with chunking and reassembly
- User heartbeat system for presence detection

## Security Considerations
- The application requires elevated privileges to access raw sockets
- All messages are encrypted using AES, but the key is hardcoded for demonstration purposes
- In a production environment, proper key management should be implemented

## Contributing
Contributions are welcome! Please feel free to submit a Pull Request.

## 📧 Contact

* Rlianny - revelianny10@gmail.com
* KevinTorres01 - kevintorresperera@gmail.com

Link del Proyecto: [https://github.com/Rlianny/Link-Chat_2025](https://github.com/Rlianny/Link-Chat_2025)
