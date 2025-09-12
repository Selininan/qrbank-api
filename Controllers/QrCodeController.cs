using Microsoft.AspNetCore.Mvc;
using QrBankApi.Helpers;
using System;
using QrBankApi.Models;
using Newtonsoft.Json.Linq;
using QrBankApi.Services.Abstractions;

namespace QrBankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        public IActionResult GenerateQr([FromBody] QrGenerateRequest request)
        {
            if (request is null)
                return BadRequest(new QrGenerateResponse
                {
                    ResponseCode = "400",
                    ResponseDescription = "Geçersiz istek"
                });

            try
            {
                // Request loglama
                var requestJson = JObject.FromObject(request);
                Console.WriteLine($"[GenerateQr] Request JSON: {requestJson}");

                // Servisten QR kodu üret
                string qrCode = _qrService.Generate(request);

                if (string.IsNullOrWhiteSpace(qrCode))
                {
                    return StatusCode(500, new QrGenerateResponse
                    {
                        ResponseCode = "500",
                        ResponseDescription = "QR kod üretilemedi"
                    });
                }

                // Response objesi oluştur
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


