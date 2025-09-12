namespace QrBankApi.Models
{
    public class QrGenerateResponse : BaseResponse
    {
        public string QrCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class QrValidateResponse : BaseResponse
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class QrWithdrawResponse : BaseResponse
    {
        public decimal Amount { get; set; }
        public string Message { get; set; } = "Para çekme işlemi başarılı";
    }

    public class QrDepositResponse : BaseResponse
    {
        public decimal Amount { get; set; }
        public string Message { get; set; } = "Para yatırma işlemi başarılı";
    }
}
