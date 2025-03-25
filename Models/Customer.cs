using GoPay.Common;

namespace GoPayCardPayment.Models
{
    /// <summary>
    /// Objekt reprezentující zákazníka (obsahuje jeho údaje), který přes GoPay provede platbu kreditní kartou
    /// </summary>
    public class Customer
    {
        public int CustomerPK { get; set; }                         // Primární klíč zákazníka v Postgres tabulce customer
        public string FirstName { get; set; } = string.Empty;       // Křestní jméno zákazníka
        public string LastName { get; set; } = string.Empty;        // Přijmení zákazníka
        public string Email { get; set; } = string.Empty;           // Email zákazníka
        public string PhoneNumber { get; set; } = string.Empty;     // Telefonní číslo zákazníka
        public string City { get; set; } = string.Empty;            // Město zákazníka
        public string Street { get; set; } = string.Empty;          // Ulice zákazníka
        public string PostalCode { get; set; } = string.Empty;      // Poštovní směrovací číslo zákazníka (PSÁT BEZ MEZERY !!!!!)
        public Country CountryCode { get; set; } = Country.CZE;     // Třípísmenný kód státu zákazníka podle standardu ISO 3166-1 alpha-3

        // Používá se ve View
        public string toStringWithoutAttrNames()
        {
            return ($"{CustomerPK}, {FirstName}, {LastName}, {Email}, {PhoneNumber}, " +
                $"{City}, {Street}, {PostalCode}, {CountryCode}\n"
            );
        }
    }
}
