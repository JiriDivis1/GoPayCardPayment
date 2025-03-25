using GoPay.Common;

namespace GoPayCardPayment.Models
{
    /// <summary>
    /// Tento model představuje platební kartu, kterou plátce používá v goPay
    /// Získá se z dotazu na stav platby a uloží se do Postgres tabulky payment_card
    /// </summary>
    public class PaymentCard
    {
        public int? paymentCardPK { get; set; }                             // Primární klíč v Postgres tabulce payment_card, null -> ještě není uložena, nebo PK ještě není zjištěn
        public string cardNumber { get; set; } = string.Empty;              // Maskovaný PAN (číslo karty)
        public string cardExpiration { get; set; } = string.Empty;          // Datum expirace (např. 1230)
        public string cardBrand { get; set; } = string.Empty;               // Asociace platební karty
        public Country cardIssuerCountry { get; set; } = Country.CZE;       // Země vydání platební karty (např. CZE)
        public string cardIssuerBank { get; set; } = string.Empty;          // Vydavatelská banka
        public string? cardFingerprint { get; set; } = null;                // Unikátní identifikátor konkrétní platební karty (nepovinný)

    }
}
