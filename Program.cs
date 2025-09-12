using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QrBankApi.Services.Abstractions;
using QrBankApi.Helpers;
using QrBankApi.Services.Implementations;
using QrBankApi.Middlewares;
using NLog.Web;
using NLog;
using System;

namespace QrBankApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // NLog setup
            NLog.LogManager.Setup().LoadConfigurationFromAppSettings();
            var logger = NLog.LogManager.GetCurrentClassLogger();

            try
            {
                logger.Debug("Init Main");

                var builder = WebApplication.CreateBuilder(args);

                // NLog’u ASP.NET Core ile kullan
                builder.Logging.ClearProviders();
                builder.Host.UseNLog();

                // Services
                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();
                
                builder.Services.AddMemoryCache();

                builder.Services.AddSingleton<ICheckDigitHelper, CheckDigitHelper>();
                builder.Services.AddScoped<IQrService, QrService>();

                var app = builder.Build();

                // HTTP pipeline
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                        c.RoutePrefix = "swagger";
                    });
                }

                app.UseMiddleware<LoggingMiddleware>(); // 🔹 Logging

                app.UseHttpsRedirection();

                app.MapControllers(); // 🔹 Controller route’ları

                app.Run(); // 🔹 Uygulama başlat
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }
    }
}
