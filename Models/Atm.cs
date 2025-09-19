namespace QrBankApi.Models
{
    public class Atm
    {
        public string BankCode { get; set; } = string.Empty;
        public string AtmCode { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
