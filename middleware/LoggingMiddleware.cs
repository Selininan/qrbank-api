using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.Json;
using System.Net;
using QrBankApi.Models;

namespace QrBankApi.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                // 🟢 1️⃣ Request logla (Method + Path)
                _logger.LogInformation($"[REQUEST] {context.Request.Method} {context.Request.Path}");

                // 🟢 2️⃣ Request Body'yi oku ve logla
                if (context.Request.Method == HttpMethods.Post || context.Request.Method == HttpMethods.Put)
                {
                    // Request body'yi yeniden okunabilir hale getirmek için buffering açıyoruz
                    context.Request.EnableBuffering();

                    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                    string body = await reader.ReadToEndAsync();

                    // Stream'i başa sarıyoruz ki diğer middleware/controller okuyabilsin
                    context.Request.Body.Position = 0;

                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        _logger.LogInformation($"[REQUEST BODY] {body}");
                    }
                }

                await _next(context);

                // 🟢 3️⃣ Response logla (StatusCode)
                _logger.LogInformation($"[RESPONSE] Status: {context.Response.StatusCode}");
            }
            catch (Exception ex)
            {
                // 🔴 Hata logla
                _logger.LogError(ex, $"[ERROR] {ex.Message}");

                var response = new BaseResponse
                {
                    ResponseCode = ((int)HttpStatusCode.InternalServerError).ToString(),
                    ResponseDescription = "Sunucu hatası oluştu.",
                    ExceptionDetails = ex.Message
                };

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var json = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(json);
            }
        }
    }
}
