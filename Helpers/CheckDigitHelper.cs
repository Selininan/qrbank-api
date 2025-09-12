using System;   
namespace QrBankApi.Helpers
{
    public class CheckDigitHelper : ICheckDigitHelper
    {
        public int CalculateCheckDigit(string input)
        {
            if (string.IsNullOrEmpty(input)) return 0;

            int sum = 0;
            foreach (var c in input)
                sum += (int)c;

            return sum % 100;
        }
    }
}



