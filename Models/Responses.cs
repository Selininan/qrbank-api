using Newtonsoft.Json;

namespace QrBankApi.Models
{
    public class QrGenerateResponse : BaseResponse
    {
        [JsonProperty("qrCode", NullValueHandling = NullValueHandling.Ignore)]
        public string? QrCode { get; set; }

        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string? Message { get; set; }

        [JsonProperty("exceptionDetails", NullValueHandling = NullValueHandling.Ignore)]
        public string? ExceptionDetails { get; set; }

        public string QrImageBase64 { get; set; } = string.Empty;
    }

    public class QrWithdrawResponse : BaseResponse
    {
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string? Message { get; set; } = "Para çekme işlemi başarılı";
    }

    public class QrDepositResponse : BaseResponse
    {
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string? Message { get; set; } = "Para yatırma işlemi başarılı";
    }
}
