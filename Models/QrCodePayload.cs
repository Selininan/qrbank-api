namespace QrBankApi.Models
{
    public class QrCodePayload
    {
        public string BankCode { get; set; } = "000001";
        public string AtmCode { get; set; } = "00111A";
        public string TransactionId { get; set; } = "11111111111111111111111111111111111111"; // Guid
        public DateTime TransactionDateTime { get; set; }
        public int CheckDigit { get; set; }
    }
}
