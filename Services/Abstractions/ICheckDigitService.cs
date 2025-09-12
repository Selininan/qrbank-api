namespace QrBankApi.Services.Abstractions
{
    public interface ICheckDigitService
    {
        /// <summary>Payload string'inden 0-99 arası int değer döner.</summary>
        int ComputeCheckDigitValue(string payload);

        /// <summary>İki haneli pad'lenmiş string döner: "05", "42" vb.</summary>
        string ComputeCheckDigitString(string payload);

        /// <summary>
        /// Eğer payloadWithCheck sonuna check eklendiyse onu ayırır.
        /// Dönen bool: başarılıysa true, payload ve check out parametrelere konur.
        /// </summary>
        bool TrySplitPayloadAndCheck(string payloadWithCheck, out string payload, out string check);
    }
}
