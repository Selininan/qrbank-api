using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using QrBankApi.Middlewares; // LoggingMiddleware namespace
using QrBankApi.Services.Abstractions;
using QrBankApi.Helpers;
using QrBankApi.Services.Implementations;
using FluentValidation.AspNetCore;
using NLog.Web;

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
// Controllers + FluentValidation
// -------------------------
builder.Services.AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Program>());

// -------------------------
// Dependency Injection
// -------------------------
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICheckDigitHelper, CheckDigitHelper>();
builder.Services.AddScoped<IQrService, QrService>();

// -------------------------
// Swagger
// -------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -------------------------
// Build App
// -------------------------
var app = builder.Build();

// -------------------------
// HTTP Pipeline
// -------------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// 🔹 Sıralama çok önemli
app.UseAuthentication();              // 1️⃣ JWT doğrulama
app.UseMiddleware<LoggingMiddleware>(); // 2️⃣ Logging
app.UseAuthorization();               // 3️⃣ Role/Policy

app.MapControllers();

app.Run();
