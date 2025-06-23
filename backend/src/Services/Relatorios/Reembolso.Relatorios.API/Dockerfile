# Estágio de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar arquivos de projeto
COPY ["src/Services/Relatorios/Reembolso.Relatorios.API/Reembolso.Relatorios.API.csproj", "src/Services/Relatorios/Reembolso.Relatorios.API/"]
COPY ["src/Shared/Reembolso.Shared/Reembolso.Shared.csproj", "src/Shared/Reembolso.Shared/"]

# Restaurar dependências
RUN dotnet restore "src/Services/Relatorios/Reembolso.Relatorios.API/Reembolso.Relatorios.API.csproj"

# Copiar código fonte
COPY . .
WORKDIR "/src/src/Services/Relatorios/Reembolso.Relatorios.API"

# Build da aplicação
RUN dotnet build "Reembolso.Relatorios.API.csproj" -c Release -o /app/build

# Estágio de publicação
FROM build AS publish
RUN dotnet publish "Reembolso.Relatorios.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Instalar dependências para geração de PDF
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    && rm -rf /var/lib/apt/lists/*

# Criar diretório para relatórios
RUN mkdir -p /app/wwwroot/relatorios

# Copiar arquivos publicados
COPY --from=publish /app/publish .

# Configurar usuário não-root
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Expor porta
EXPOSE 8080

# Configurar variáveis de ambiente
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Ponto de entrada
ENTRYPOINT ["dotnet", "Reembolso.Relatorios.API.dll"]