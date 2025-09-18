using QrBankApi.Services.Abstractions;
using System;
using System.Linq;

namespace QrBankApi.Services
{
    public class ValidateQrService : IValidateQrService
    {
        // Geçerli banka kodları
        private static readonly string[] ValidBankCodes = { "980015", "980020", "980013", "980017" };

        public bool IsValid(string atmCode, string bankCode, out string validationError)
        {
            validationError = string.Empty;

            if (string.IsNullOrWhiteSpace(bankCode))
            {
                validationError = "Banka kodu boş olamaz.";
                return false;
            }

            if (!ValidBankCodes.Contains(bankCode))
            {
                validationError = $"Geçersiz banka kodu: {bankCode}";
                return false;
            }

            if (string.IsNullOrWhiteSpace(atmCode))
            {
                validationError = "ATM kodu boş olamaz.";
                return false;
            }

            if (atmCode.Length != 6)
            {
                validationError = "ATM kodu 6 haneli olmalıdır.";
                return false;
            }

            return true;
        }

        public string GenerateQrCode(string bankCode, string atmCode)
        {
            var guidPart = Guid.NewGuid().ToString("N").Substring(0, 10);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            return $"{bankCode}{atmCode}{guidPart}{timestamp}";
        }
    }
}
