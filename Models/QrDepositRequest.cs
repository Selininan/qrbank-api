namespace QrBankApi.Models
{
    public class QrDepositRequest
    {
        public required string QrCode { get; set; }
        public decimal Amount { get; set; }
    }

}
