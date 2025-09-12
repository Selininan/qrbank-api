namespace QrBankApi.Models
{
    public class QrWithdrawRequest
    {
        public required string QrCode { get; set; }
        public decimal Amount { get; set; }
    }
}