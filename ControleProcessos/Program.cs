using ControleProcessos.Interfaces;
using ControleProcessos.Services;
using ControleProcessos.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Port={uri.Port};" +
                       $"Database={uri.AbsolutePath.TrimStart('/')};" +
                       $"Username={userInfo[0]};Password={userInfo[1]};" +
                       $"SSL Mode=Require;Trust Server Certificate=true";
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
               "Connection string nao configurada. Defina DATABASE_URL ou " +
               "ConnectionStrings__DefaultConnection no Render.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IProcessoService, ProcessoService>();
builder.Services.AddScoped<IMovimentacaoService, MovimentacaoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
           "JWT Key nao configurada. Defina Jwt__Key .");

var key = Encoding.ASCII.GetBytes(jwtKey);

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
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

var aspnetcorePort = Environment.GetEnvironmentVariable("ASPNETCORE_PORT");
if (aspnetcorePort == null)
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();


app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();