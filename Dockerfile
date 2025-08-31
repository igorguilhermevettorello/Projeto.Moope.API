# Use a imagem oficial do .NET 8.0 SDK para build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar arquivos de projeto e restaurar dependências
COPY ["Projeto.Moope.API/Projeto.Moope.API.csproj", "Projeto.Moope.API/"]
COPY ["Projeto.Moope.Core/Projeto.Moope.Core.csproj", "Projeto.Moope.Core/"]
COPY ["Projeto.Moope.Infrastructure/Projeto.Moope.Infrastructure.csproj", "Projeto.Moope.Infrastructure/"]

# Restaurar dependências
RUN dotnet restore "Projeto.Moope.API/Projeto.Moope.API.csproj"

# Copiar todo o código fonte
COPY . .

# Build da aplicação
WORKDIR "/src/Projeto.Moope.API"
RUN dotnet build "Projeto.Moope.API.csproj" -c Release -o /app/build

# Publish da aplicação
FROM build AS publish
RUN dotnet publish "Projeto.Moope.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagem final de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Instalar MySQL client para health checks
RUN apt-get update && apt-get install -y default-mysql-client curl && rm -rf /var/lib/apt/lists/*

# Copiar arquivos publicados
COPY --from=publish /app/publish .

# Expor porta
EXPOSE 80
EXPOSE 443

# Configurar entrypoint
ENTRYPOINT ["dotnet", "Projeto.Moope.API.dll"]
