namespace QrBankApi.Services.Abstractions
{
    public interface IValidateQrService
    {
        bool IsValid(string bankCode, string atmCode, out string validationError);
        string GenerateQrCode(string bankCode, string atmCode);
    }
}
