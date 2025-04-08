# Acesse https://aka.ms/customizecontainer para saber como personalizar seu contêiner de depuração e como o Visual Studio usa este Dockerfile para criar suas imagens para uma depuração mais rápida.
# Esta fase é usada durante a execução no VS no modo rápido (Padrão para a configuração de Depuração)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 80

# Esta fase é usada para compilar o projeto de serviço
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

# Esta fase é usada para publicar o projeto de serviço a ser copiado para a fase final
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ControlApp.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase é usada na produção ou quando executada no VS no modo normal (padrão quando não está usando a configuração de Depuração)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ControlApp.API.dll"]