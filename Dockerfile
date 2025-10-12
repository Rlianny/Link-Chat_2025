# --- Etapa de Build ---
# Usamos la imagen del SDK de .NET 9 para compilar el proyecto.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiamos todos los archivos del proyecto al contenedor.
# El .dockerignore se encargará de excluir las carpetas innecesarias.
COPY . .

# Restauramos las dependencias y publicamos la aplicación de consola en modo Release.
RUN dotnet restore LinkChat.ConsoleApp/LinkChat.ConsoleApp.csproj
RUN dotnet publish LinkChat.ConsoleApp/LinkChat.ConsoleApp.csproj -c Release -o /app/publish --no-restore

# --- Etapa de Runtime ---
# Usamos una imagen más ligera que solo contiene el runtime de .NET.
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app

# Instalamos paquetes de red útiles para diagnóstico.
RUN apt-get update && apt-get install -y --no-install-recommends \
    iproute2 \
    tcpdump \
    ethtool \
    && rm -rf /var/lib/apt/lists/*

# Copiamos la aplicación compilada desde la etapa de build.
COPY --from=build /app/publish .

# Tu aplicación necesita permisos de root para usar sockets AF_PACKET.
USER root

# Definimos el comando que se ejecutará al iniciar el contenedor.
ENTRYPOINT ["./LinkChat.ConsoleApp"]
