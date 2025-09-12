using QrBankApi.Services.Abstractions;
using System;

namespace QrBankApi.Services.Implementations
{
    public class CheckDigitService : ICheckDigitService
    {
        public int ComputeCheckDigitValue(string payload)
        {
            if (string.IsNullOrEmpty(payload)) return 0;

            // ASCII toplamı
            int sum = 0;
            foreach (var c in payload)
            {
                sum += (int)c; // char -> ASCII/int
            }

            return sum % 100;
        }

        public string ComputeCheckDigitString(string payload)
        {
            int v = ComputeCheckDigitValue(payload);
            return v.ToString("D2"); // 2 haneli pad
        }

        public bool TrySplitPayloadAndCheck(string payloadWithCheck, out string payload, out string check)
        {
            payload = string.Empty;
            check = string.Empty;

            if (string.IsNullOrEmpty(payloadWithCheck) || payloadWithCheck.Length < 2)
                return false;

            // Son 2 karakter check (ör: "05")
            int len = payloadWithCheck.Length;
            check = payloadWithCheck.Substring(len - 2, 2);
            payload = payloadWithCheck.Substring(0, len - 2);

            // check'in numeric olup olmadığını kontrol et
            return int.TryParse(check, out _);
        }
    }
}
