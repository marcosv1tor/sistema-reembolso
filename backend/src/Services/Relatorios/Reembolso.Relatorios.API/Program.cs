using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Reembolso.Relatorios.API.Data;
using Reembolso.Relatorios.API.Mappings;
using Reembolso.Relatorios.API.Services;
using Reembolso.Shared.Middleware;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuração do banco de dados
builder.Services.AddDbContext<RelatoriosDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuração do AutoMapper
builder.Services.AddAutoMapper(typeof(RelatorioProfile));

// Configuração da autenticação JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Configuração da autorização
builder.Services.AddAuthorization();

// Registro dos serviços
builder.Services.AddScoped<IRelatorioService, RelatorioService>();

// Configuração do HttpClient para comunicação entre microserviços
builder.Services.AddHttpClient();

// Configuração do CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configuração dos controllers
builder.Services.AddControllers();

// Configuração do Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Reembolso - API de Relatórios",
        Version = "v1",
        Description = "API para gerenciamento de relatórios do sistema de reembolso",
        Contact = new OpenApiContact
        {
            Name = "Equipe de Desenvolvimento",
            Email = "dev@reembolso.com"
        }
    });

    // Configuração da autenticação JWT no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Incluir comentários XML
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configuração de logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configuração de health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<RelatoriosDbContext>();

var app = builder.Build();

// Aplicar migrações automaticamente
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<RelatoriosDbContext>();
    try
    {
        context.Database.Migrate();
        app.Logger.LogInformation("Migrações aplicadas com sucesso");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Erro ao aplicar migrações");
    }
}

// Configuração do pipeline de requisições
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Reembolso - API de Relatórios v1");
        c.RoutePrefix = string.Empty; // Swagger na raiz
    });
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Middleware de tratamento de exceções
app.UseMiddleware<ExceptionMiddleware>();

// Configuração de arquivos estáticos para servir relatórios
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Health checks
app.MapHealthChecks("/health");

// Mapeamento dos controllers
app.MapControllers();

// Endpoint de informações da API
app.MapGet("/", () => new
{
    Service = "Reembolso - API de Relatórios",
    Version = "1.0.0",
    Environment = app.Environment.EnvironmentName,
    Timestamp = DateTime.UtcNow
});

// Configuração de tarefas em background para limpeza de relatórios antigos
if (!app.Environment.IsDevelopment())
{
    var timer = new Timer(async _ =>
    {
        using var scope = app.Services.CreateScope();
        var relatorioService = scope.ServiceProvider.GetRequiredService<IRelatorioService>();
        try
        {
            await relatorioService.LimparRelatoriosAntigosAsync();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Erro na limpeza automática de relatórios antigos");
        }
    }, null, TimeSpan.FromHours(1), TimeSpan.FromHours(24)); // Executa a cada 24 horas, começando em 1 hora
}

app.Logger.LogInformation("API de Relatórios iniciada em {Environment}", app.Environment.EnvironmentName);

app.Run();
// ... existing code ...