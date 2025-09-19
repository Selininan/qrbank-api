using Microsoft.Extensions.Caching.Memory;
using System;
using QrBankApi.Models;
using QrBankApi.Services.Abstractions;
using QrBankApi.Helpers;
using QRCoder;
using System.IO;
using SkiaSharp;
using System;

namespace QrBankApi.Services.Implementations
{
    public class QrService : IQrService
    {
        private readonly ICheckDigitHelper _checkDigitHelper;
        private readonly IMemoryCache _cache; //  MemoryCache eklendi

        public QrService(ICheckDigitHelper checkDigitHelper, IMemoryCache cache)
        {
            _checkDigitHelper = checkDigitHelper;
            _cache = cache;
        }

   

        public string GenerateQrImageBase64(string qrCodeText)
        {
            if (string.IsNullOrWhiteSpace(qrCodeText))
                return string.Empty;

            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(qrCodeText, QRCodeGenerator.ECCLevel.Q);

            using var qrCode = new PngByteQRCode(qrData);
            byte[] qrBytes = qrCode.GetGraphic(20); // 20 pixel scale

            return Convert.ToBase64String(qrBytes);
        }

        public string Generate(QrGenerateRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // 1️ Her generate isteğinde uniq cache key
            string uniqueKey = Guid.NewGuid().ToString("N");
            string cacheKey = $"{request.BankCode}_{request.AtmCode}_{uniqueKey}";

            // 2️ QR payload oluştur
            string transactionId = Guid.NewGuid().ToString("N");
            string transactionDate = DateTime.Now.ToString("yyyyMMddHHmmss");
            string qrPayload = $"{request.BankCode}{request.AtmCode}{transactionId}{transactionDate}";
            int checkDigit = _checkDigitHelper.CalculateCheckDigit(qrPayload);
            string qrCode = $"{qrPayload}{checkDigit:D2}";

            // 3️ Cache’e ekle (örn. 2 dakika geçerli)
            _cache.Set(cacheKey, new { Request = request, QrCode = qrCode }, TimeSpan.FromMinutes(2));

            return qrCode;
        }


        public bool Validate(QrValidateRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.QrCode))
                return false;

            var parts = request.QrCode.Split('|');
            if (parts.Length < 5) return false;

            string providedCheckDigit = parts[^1];
            string qrPayload = string.Join("|", parts, 0, parts.Length - 1);

            int expectedCheckDigit = _checkDigitHelper.CalculateCheckDigit(qrPayload);

            return providedCheckDigit == expectedCheckDigit.ToString("D2");
        }

        public string Withdraw(QrWithdrawRequest request)
        {
            return $"Withdraw {request.Amount} başarılı";
        }

        public string Deposit(QrDepositRequest request)
        {
            return $"Deposit {request.Amount} başarılı";
        }

        public int CalculateCheckDigitForPayload(string payload)
        {
            return _checkDigitHelper.CalculateCheckDigit(payload);
        }
    }
}
