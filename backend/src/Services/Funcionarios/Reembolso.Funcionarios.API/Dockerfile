FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar arquivos de projeto
COPY ["backend/src/Services/Funcionarios/Reembolso.Funcionarios.API/Reembolso.Funcionarios.API.csproj", "backend/src/Services/Funcionarios/Reembolso.Funcionarios.API/"]
COPY ["backend/src/Shared/Reembolso.Shared/Reembolso.Shared.csproj", "backend/src/Shared/Reembolso.Shared/"]

# Restaurar dependências
RUN dotnet restore "backend/src/Services/Funcionarios/Reembolso.Funcionarios.API/Reembolso.Funcionarios.API.csproj"

# Copiar código fonte
COPY backend/src/ backend/src/

# Build da aplicação
WORKDIR "/src/backend/src/Services/Funcionarios/Reembolso.Funcionarios.API"
RUN dotnet build "Reembolso.Funcionarios.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Reembolso.Funcionarios.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Reembolso.Funcionarios.API.dll"]