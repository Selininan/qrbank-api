using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using QrBankApi.Helpers;
using QrBankApi.Models;
using QrBankApi.Services.Abstractions;
using System;
using System.Linq;

namespace QrBankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Controller düzeyinde default olarak token gerekli
    public class QrCodeController : ControllerBase
    {
        private readonly IQrService _qrService;
        private readonly ICheckDigitHelper _checkDigitHelper;

        public QrCodeController(IQrService qrService, ICheckDigitHelper checkDigitHelper)
        {
            _qrService = qrService ?? throw new ArgumentNullException(nameof(qrService));
            _checkDigitHelper = checkDigitHelper ?? throw new ArgumentNullException(nameof(checkDigitHelper));
        }

        [HttpPost("generate")]
        [AllowAnonymous] // Token varsa kontrol edilecek, yoksa anonymous
        public IActionResult GenerateQr([FromBody] QrGenerateRequest request)
        {
            // Kullanıcı adı ve rol kontrolü
            var username = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "anonymous";
            var isAdmin = User.Identity?.IsAuthenticated == true && User.IsInRole("Admin");

            // Request null mu kontrol
            if (request == null)
            {
                return BadRequest(new QrGenerateResponse
                {
                    ResponseCode = "400",
                    ResponseDescription = "Geçersiz istek"
                });
            }

            // FluentValidation hatalarını kontrol
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new QrGenerateResponse
                {
                    ResponseCode = "400",
                    ResponseDescription = string.Join(" | ", errors)
                });
            }

            try
            {
                // Request loglama
                var requestJson = JObject.FromObject(request);
                Console.WriteLine($"[GenerateQr] Kullanıcı: {username} - Admin mi: {isAdmin} - Request: {requestJson}");

                // QR kodu üret
                string qrCode = _qrService.Generate(request);

                if (string.IsNullOrWhiteSpace(qrCode))
                {
                    return StatusCode(500, new QrGenerateResponse
                    {
                        ResponseCode = "500",
                        ResponseDescription = "QR kod üretilemedi"
                    });
                }

                var response = new QrGenerateResponse
                {
                    ResponseCode = "200",
                    ResponseDescription = "QR kodu üretildi",
                    QrCode = qrCode
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GenerateQr] Hata: {ex.Message}");

                return StatusCode(500, new QrGenerateResponse
                {
                    ResponseCode = "500",
                    ResponseDescription = "Beklenmeyen bir hata oluştu",
                    ExceptionDetails = ex.Message
                });
            }
        }
    }
}

