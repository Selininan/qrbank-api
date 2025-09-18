using Microsoft.Extensions.Logging;
using System;

namespace QrBankApi.Helpers
{
    public static class LoggerExtensions
    {
        public static void LogGenerateQrStart(this ILogger logger, string atmCode, string bankCode)
        {
            logger.LogTrace("[GenerateQr] Başladı - ATM: {atm}, Banka: {bank}", atmCode, bankCode);
        }

        public static void LogGenerateQrSuccess(this ILogger logger, string qrCode)
        {
            logger.LogInformation("[GenerateQr] Başarılı - QR: {qr}", qrCode);
        }

        public static void LogValidateQrStart(this ILogger logger, string qrCode, string user)
        {
            logger.LogTrace("[ValidateQr] Başladı - Kullanıcı: {user}, QR: {qr}", user, qrCode);
        }

        public static void LogValidateQrSuccess(this ILogger logger, string qrCode)
        {
            logger.LogInformation("[ValidateQr] QR doğrulandı ve cache'ten silindi - QR: {qr}", qrCode);
        }

        public static void LogQrValidationError(this ILogger logger, string context, string message)
        {
            logger.LogWarning("[{context}] Geçersiz QR: {msg}", context, message);
        }

        public static void LogQrException(this ILogger logger, Exception ex, string context)
        {
            logger.LogError(ex, "[{context}] Hata oluştu: {msg}", context, ex.Message);
        }
    }
}
