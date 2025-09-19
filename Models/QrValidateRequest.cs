namespace QrBankApi.Models
{
    public class QrValidateRequest
    {
        public required string QrCode { get; set; }
        public double Latitude { get; set; }   
        public double Longitude { get; set; }
    }

   
}