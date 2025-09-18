using FluentValidation;
using Newtonsoft.Json;
using System.Linq;

namespace QrBankApi.Models
{
    public class QrGenerateRequest
    {
        [JsonProperty("AtmCode")]
        public string AtmCode { get; set; }

        [JsonProperty("BankCode")]
        public string BankCode { get; set; }
    }

    public class QrGenerateRequestValidator : AbstractValidator<QrGenerateRequest>
    {
        // Geçerli banka kodları
        private static readonly string[] ValidBankCodes = { "980015", "980020", "980013", "980017" };

        public QrGenerateRequestValidator()
        {
            RuleFor(x => x.AtmCode)
                .Length(6)
                .WithMessage("ATM kodu maksimum 6 haneli olmalıdır.");

            RuleFor(x => x.BankCode)
                .Must(code => ValidBankCodes.Contains(code))
                .WithMessage("Geçersiz banka kodu. Geçerli kodlar: 980015, 980020, 980013, 980017.");
        }
    }
}


