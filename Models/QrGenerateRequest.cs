using FluentValidation;
using System.Linq;

namespace QrBankApi.Models
{
    // Request Model
    public class QrGenerateRequest
    {
        public string BankCode { get; set; } = string.Empty;
        public string AtmCode { get; set; } = string.Empty;
    }

    // Validator
    public class QrGenerateRequestValidator : AbstractValidator<QrGenerateRequest>
    {
        // Geçerli banka kodları
        private static readonly string[] ValidBankCodes = { "980015", "980020", "980013", "980017" };

        public QrGenerateRequestValidator()
        {
            // ATM kodu en fazla 6 haneli olmalı
            RuleFor(x => x.AtmCode)
                .Length(6)
                .WithMessage("ATM kodu maksimum 6 haneli olmalıdır.");

            // Banka kodu geçerli mi kontrolü
            RuleFor(x => x.BankCode)
                .Must(code => ValidBankCodes.Contains(code))
                .WithMessage("Geçersiz banka kodu. Geçerli kodlar: 980015, 980020, 980013, 980017.");
        }
    }
}



