using Microsoft.Extensions.Caching.Memory;
using System;
using QrBankApi.Models;
using QrBankApi.Services.Abstractions;
using QrBankApi.Helpers;

namespace QrBankApi.Services.Implementations
{
    public class QrService : IQrService
    {
        private readonly ICheckDigitHelper _checkDigitHelper;
        private readonly IMemoryCache _cache; // ✅ MemoryCache eklendi

        public QrService(ICheckDigitHelper checkDigitHelper, IMemoryCache cache)
        {
            _checkDigitHelper = checkDigitHelper;
            _cache = cache;
        }

        public string Generate(QrGenerateRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // ✅ Cache key: BankCode + AtmCode + Amount (veya request’te seni eşsiz tanımlayacak alanlar)
            string cacheKey = $"{request.BankCode}_{request.AtmCode}";

            // 1️⃣ Önce cache’de var mı kontrol et
            if (_cache.TryGetValue(cacheKey, out string cachedQr))
            {
                return cachedQr; // Cache’de varsa direkt dön
            }

            // 2️⃣ Yoksa yeni QR üret
            string transactionId = Guid.NewGuid().ToString("N");
            string transactionDate = DateTime.Now.ToString("yyyyMMddHHmmss");

            string qrPayload = $"{request.BankCode}{request.AtmCode}{transactionId}{transactionDate}";
            int checkDigit = _checkDigitHelper.CalculateCheckDigit(qrPayload);

            string qrCode = $"{qrPayload}|{checkDigit:D2}";

            // 3️⃣ Cache’e koy (örn. 5 dakika geçerli olsun)
            _cache.Set(cacheKey, qrCode, TimeSpan.FromMinutes(5));

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
