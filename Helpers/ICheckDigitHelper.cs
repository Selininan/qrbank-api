// ICheckDigitHelper.cs
namespace QrBankApi.Helpers
{
    public interface ICheckDigitHelper
    {
        /// <summary>ASCII toplamının 100 modunu döner (0..99)</summary>
        int CalculateCheckDigit(string input);
    }
}

public interface ICheckDigitHelper
{
    int CalculateCheckDigit(string input);
}
public class CheckDigitHelper : ICheckDigitHelper
{
    public int CalculateCheckDigit(string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input boş olamaz");

        int sum = 0;
        foreach (char c in input)
        {
            // karakterin ASCII değerini alıyoruz
            sum += (int)c;
        }

        return sum % 100; // 100'e göre mod al
    }
}


