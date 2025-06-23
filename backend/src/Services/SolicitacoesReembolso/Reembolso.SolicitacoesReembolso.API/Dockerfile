# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["backend/src/Services/SolicitacoesReembolso/Reembolso.SolicitacoesReembolso.API/Reembolso.SolicitacoesReembolso.API.csproj", "backend/src/Services/SolicitacoesReembolso/Reembolso.SolicitacoesReembolso.API/"]
COPY ["backend/src/Shared/Reembolso.Shared/Reembolso.Shared.csproj", "backend/src/Shared/Reembolso.Shared/"]
RUN dotnet restore "./backend/src/Services/SolicitacoesReembolso/Reembolso.SolicitacoesReembolso.API/Reembolso.SolicitacoesReembolso.API.csproj"
COPY . .
WORKDIR "/src/backend/src/Services/SolicitacoesReembolso/Reembolso.SolicitacoesReembolso.API"
RUN dotnet build "./Reembolso.SolicitacoesReembolso.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Reembolso.SolicitacoesReembolso.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Reembolso.SolicitacoesReembolso.API.dll"]