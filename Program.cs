using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using QrBankApi.Middlewares; // LoggingMiddleware namespace
using QrBankApi.Services.Abstractions;
using QrBankApi.Helpers;
using QrBankApi.Services.Implementations;
using QrBankApi.Services;
using FluentValidation.AspNetCore;
using NLog.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.Extensions.Caching.Memory;


var builder = WebApplication.CreateBuilder(args);

// -------------------------
// Logging / NLog
// -------------------------
builder.Host.UseNLog();

// -------------------------
// JWT Authentication
// -------------------------
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key missing"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // dev ortamı
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = ctx =>
        {
            var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("JWT validasyonu başarılı: {user}", ctx.Principal?.Identity?.Name);
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = ctx =>
        {
            var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(ctx.Exception, "JWT doğrulama hatası");
            return Task.CompletedTask;
        }
    };
});

// -------------------------
// Authorization (role / policy)
// -------------------------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// -------------------------
// Controllers + FluentValidation + NewtonsoftJson
// -------------------------
builder.Services.AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Program>())
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    });

// -------------------------
// Swagger
// -------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Atm Dependency Injection
builder.Services.AddSingleton<IAtmCache, AtmCache>();

// -------------------------
// Dependency Injection
// -------------------------
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICheckDigitHelper, CheckDigitHelper>();
builder.Services.AddScoped<IQrService, QrService>();
builder.Services.AddScoped<IValidateQrService, ValidateQrService>();

// -------------------------
// Build App
// -------------------------
var app = builder.Build();

// -------------------------
// HTTP Pipeline
// -------------------------
app.UseHttpsRedirection();
app.UseAuthentication();
// app.UseMiddleware<LoggingMiddleware>(); // İstersen açabilirsin
app.UseAuthorization();

// Swagger middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger(); // JSON endpoint için gerekli
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = "swagger"; // UI için https://localhost:7239/swagger/
    });
}

// MemoryCache için gerekli
var memoryCache = new MemoryCache(new MemoryCacheOptions());

// CheckDigitHelper sınıfını kendi projenle eşleştir
var checkDigitHelper = new CheckDigitHelper();

// QrService’i oluştur
var qrService = new QrService(checkDigitHelper, memoryCache);

// Test QR kodunu üret
string qrBase64 = qrService.GenerateQrImageBase64("TEST12345");

// Base64 → byte[]
byte[] qrBytes = Convert.FromBase64String(qrBase64);

// PNG dosyası olarak kaydet
string filePath = "qr_test.png";
File.WriteAllBytes(filePath, qrBytes);

Console.WriteLine($"QR kodu {filePath} olarak kaydedildi!");


app.MapControllers();

app.Run();
