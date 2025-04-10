# Fase Base: utilizada para execução do aplicativo
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Fase Build: compila o projeto
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ControlApp.API/ControlApp.API.csproj", "ControlApp.API/"]
COPY ["ControlApp.Domain/ControlApp.Domain.csproj", "ControlApp.Domain/"]
COPY ["ControlApp.Infra.Data/ControlApp.Infra.Data.csproj", "ControlApp.Infra.Data/"]
COPY ["ControlApp.Infra.Security/ControlApp.Infra.Security.csproj", "ControlApp.Infra.Security/"]
RUN dotnet restore "./ControlApp.API/ControlApp.API.csproj"
COPY . .
WORKDIR "/src/ControlApp.API"
RUN dotnet build "./ControlApp.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Fase Publish: publica a aplicação para produção
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ControlApp.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Fase Final: imagem de produção
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Muda para root para criar o diretório e ajustar as permissões
USER root
RUN mkdir -p /app/wwwroot/images && chown -R app:app /app/wwwroot/images

# Volta ao usuário não-root (se desejar rodar como "app")
USER app

ENTRYPOINT ["dotnet", "ControlApp.API.dll"]