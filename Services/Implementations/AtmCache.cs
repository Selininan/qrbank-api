using QrBankApi.Models;
using QrBankApi.Services.Abstractions;

namespace QrBankApi.Services.Implementations
{
    public class AtmCache : IAtmCache
    {
        private readonly Dictionary<string, Atm> _atms;

        public AtmCache()
        {
            _atms = new Dictionary<string, Atm>
            {
                ["980015-123456"] = new Atm
                {
                    BankCode = "980015",
                    AtmCode = "123456",
                    Latitude = 40.99942324892275,
                    Longitude = 29.11262693439363
                }
            };
        }

        public Atm GetAtm(string bankCode, string atmCode)
        {
            _atms.TryGetValue($"{bankCode}-{atmCode}", out var atm);
            return atm;
        }
    }
}
