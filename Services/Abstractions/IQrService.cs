using QrBankApi.Models;

namespace QrBankApi.Services.Abstractions
{
    public interface IQrService
    {
        // Artık sadece string dönüyor, API response değil
        string Generate(QrGenerateRequest request);

        // Validate aslında true/false döner, JSON’a API katmanı karar verir
        bool Validate(QrValidateRequest request);

        // Withdraw & Deposit örnekte amount dönüyor (örnek basit string)
        string Withdraw(QrWithdrawRequest request);
        string Deposit(QrDepositRequest request);

        int CalculateCheckDigitForPayload(string payload);
    }
}


    