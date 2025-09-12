using QrBankApi.Models;
namespace QrBankApi.Models
{
    public class BaseResponse
    {
        public string ResponseCode { get; set; }
        public string ResponseDescription { get; set; }

        public string ExceptionDetails { get; set; } = string.Empty;
    }
}
