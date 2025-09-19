using QrBankApi.Models;
namespace QrBankApi.Services.Abstractions
{
    public interface IAtmCache
    {
        Atm? GetAtm(string bankCode, string atmCode);
    }
}
