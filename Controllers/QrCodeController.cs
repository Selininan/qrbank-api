using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrBankApi.Helpers;
using QrBankApi.Models;
using QrBankApi.Services.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace QrBankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QrCodeController : ControllerBase
    {
        private readonly IQrService _qrService;
        private readonly ICheckDigitHelper _checkDigitHelper;
        private readonly IValidateQrService _validateQrService;
        private readonly ILogger<QrCodeController> _logger;
        private readonly IMemoryCache _cache;

        public QrCodeController(
            IQrService qrService,
            ICheckDigitHelper checkDigitHelper,
            IValidateQrService validateQrService,
            IMemoryCache cache,
            ILogger<QrCodeController> logger)
        {
            _qrService = qrService ?? throw new ArgumentNullException(nameof(qrService));
            _checkDigitHelper = checkDigitHelper ?? throw new ArgumentNullException(nameof(checkDigitHelper));
            _validateQrService = validateQrService ?? throw new ArgumentNullException(nameof(validateQrService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger;
        }

        [HttpPost("generate")]
        [AllowAnonymous]
        public IActionResult GenerateQr([FromBody] QrGenerateRequest request)
        {
            if (request == null)
            {
                return BadRequest(new QrGenerateResponse
                {
                    ResponseCode = "400",
                    ResponseDescription = "Geçersiz istek"
                });
            }

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
                _logger.LogGenerateQrStart(request.AtmCode, request.BankCode);

                if (!_validateQrService.IsValid(request.AtmCode, request.BankCode, out var validationError))
                {
                    _logger.LogQrValidationError("GenerateQr", validationError);
                    return BadRequest(new QrGenerateResponse
                    {
                        ResponseCode = "400",
                        ResponseDescription = validationError
                    });
                }

                string qrCode = _qrService.Generate(request);
                if (string.IsNullOrWhiteSpace(qrCode))
                {
                    return StatusCode(500, new QrGenerateResponse
                    {
                        ResponseCode = "500",
                        ResponseDescription = "QR kod üretilemedi"
                    });
                }

                string qrImageBase64 = _qrService.GenerateQrImageBase64(qrCode);

                var cacheEntry = new
                {
                    Request = request,
                    UserName = User.Identity?.Name, // anonim ise null
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                _cache.Set(qrCode, cacheEntry, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

                _logger.LogGenerateQrSuccess(qrCode);

                return Ok(new QrGenerateResponse
                {
                    ResponseCode = "200",
                    ResponseDescription = "QR kodu üretildi ve cache'e kaydedildi",
                    QrCode = qrCode,
                    QrImageBase64 = qrImageBase64
                });
            }
            catch (Exception ex)
            {
                _logger.LogQrException(ex, "GenerateQr");
                return StatusCode(500, new QrGenerateResponse
                {
                    ResponseCode = "500",
                    ResponseDescription = "Beklenmeyen bir hata oluştu",
                    ExceptionDetails = ex.Message
                });
            }
        }

        [HttpPost("validate")]
        [Authorize]
        public IActionResult ValidateQr([FromBody] QrValidateRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.QrCode))
            {
                return BadRequest(new
                {
                    ResponseCode = "400",
                    ResponseDescription = "QR kod boş olamaz"
                });
            }

            _logger.LogValidateQrStart(request.QrCode, User.Identity?.Name ?? "anonymous");

            if (!_cache.TryGetValue(request.QrCode, out dynamic cacheEntry))
            {
                _logger.LogQrValidationError("ValidateQr", "Cache'te bulunamadı veya süresi doldu");
                return NotFound(new
                {
                    ResponseCode = "404",
                    ResponseDescription = "QR kod geçersiz veya süresi dolmuş"
                });
            }

            // Kullanıcı eşleştirme kontrolü (anonim QR ise atla)
            if (cacheEntry.UserName != null &&
                !string.Equals(cacheEntry.UserName, User.Identity?.Name, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogQrValidationError("ValidateQr", "QR kodu başka bir kullanıcı tarafından oluşturuldu");
                return Forbid();
            }

            // IP kontrolü (opsiyonel)
            var currentIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (cacheEntry.IpAddress != null && cacheEntry.IpAddress != currentIp)
            {
                _logger.LogQrValidationError("ValidateQr", "IP adresi eşleşmedi");
                return Forbid();
            }

            // ATM konum kontrolü
            var atmCache = HttpContext.RequestServices.GetRequiredService<IAtmCache>();
            var atm = atmCache.GetAtm(cacheEntry.Request.BankCode, cacheEntry.Request.AtmCode);
            if (atm == null)
            {
                _logger.LogQrValidationError("ValidateQr", "ATM bulunamadı");
                return BadRequest(new
                {
                    ResponseCode = "404",
                    ResponseDescription = "ATM bulunamadı"
                });
            }

            var distanceKm = HaversineDistance(atm.Latitude, atm.Longitude, request.Latitude, request.Longitude);
            if (distanceKm > 1)
            {
                _logger.LogQrValidationError("ValidateQr", $"Konum mesafesi çok uzak ({distanceKm:F2} km)");
                return BadRequest(new
                {
                    ResponseCode = "400",
                    ResponseDescription = "ATM'ye uzaklık 1 km'den fazla"
                });
            }

            // QR mantıksal doğrulama
            if (!_validateQrService.IsValid(cacheEntry.Request.AtmCode, cacheEntry.Request.BankCode, out string validationError))
            {
                _logger.LogQrValidationError("ValidateQr", validationError);
                return BadRequest(new
                {
                    ResponseCode = "400",
                    ResponseDescription = validationError
                });
            }

            _cache.Remove(request.QrCode);
            _logger.LogValidateQrSuccess(request.QrCode);

            return Ok(new
            {
                ResponseCode = "200",
                ResponseDescription = "QR doğrulandı ve onaylandı"
            });
        }

        /// <summary>
        /// Haversine Formülü ile iki koordinat arasındaki mesafeyi km cinsinden döner
        /// </summary>
        private static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Dünya yarıçapı (km)
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRadians(double angle) => Math.PI * angle / 180.0;
    }
}

