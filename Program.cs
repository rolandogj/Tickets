using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GlobalTech.Data;
using GlobalTech.Services;

var builder = WebApplication.CreateBuilder(args);

// Servicios CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 1. Configurar autenticación JWT
var SecretKey = builder.Configuration["JwtSettings:SecretKey"];
var keyBytes = Encoding.UTF8.GetBytes(SecretKey!);

// 2. Configurar autorización
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(config =>
    {
        config.RequireHttpsMetadata = false;
        config.SaveToken = true;
        config.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        };
    });

// 3. Permitir el acceso desla la Web
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()   // Permite que el puerto 5500 se conecte
              .AllowAnyMethod()   // Permite PUT, POST, GET, DELETE
              .AllowAnyHeader();  // Permite mandar formularios con fotos
    });
});


// 4. Configurar conexión a PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(option => option.UseNpgsql(connectionString));


// 5.Registrar los servicios
// AddScope: Crea una nueva instancia por cada petición HTTP
builder.Services.AddScoped<IJwtService, JwtService>();

// 6. Registrar servicio de controladores
builder.Services.AddControllers();
builder.Services.AddAuthorization();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var app = builder.Build();

// Pertitirnos que la API muestre archivos que estan dentro de la carpeta wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("PermitirTodo");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();