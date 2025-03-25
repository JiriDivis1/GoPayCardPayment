using GoPay.Common;
using GoPay.Model.Payments;
using static GoPay.Model.Payments.Payment;

namespace GoPayCardPayment.Models
{
    /// <summary>
    /// Vlastní/upravená verze objektu Payment (GoPay.Model.Payments.Payment), která reprezentuje platbu přes GoPay
    /// Od GoPay se liší: obsahuje PK pro uložení do Postgres, Amount má v celých korunách/eurech (GoPay payment má Amount v haléřích/centech)
    /// </summary>
    public class EshopPayment
    {
        public int? PaymentPK { get; set; }                                 // Primární klíč platby v Postgres tabulce payment, null -> ještě není uložena, nebo PK ještě není zjištěn                
        public long PaymentID { get; set; }                                 // ID platby 
        public string OrderNumber { get; set; } = string.Empty;             // Identifikace objednávky v rámci e-shopu, alfanumerické znaky
        public SessionState State { get; set; } = SessionState.CREATED;     // Stav platby 
        public PaymentInstrument PaymentInstrument { get; set; } = PaymentInstrument.PAYMENT_CARD;  // Zvolená platební metoda
        public decimal Amount { get; set; }                                  // Celková částka platby v celých korunách/eurech/...
        public Currency Currency { get; set; } = Currency.CZK;              // Měna platby (např. CZK)
        public EshopPayer Payer { get; set; } = new EshopPayer();           // Plátce (zákazník + platební karta), který tuto platbu uskutečnil
        public string Lang { get; set; } = string.Empty;                    // Jazyk na platební bráně (např. "CS")
        public string GwURL { get; set; } = string.Empty;                   // URL platební brány

        public override string ToString()
        {
            return (
                $"PaymentID = {this.PaymentID}, OrderNumber = {this.OrderNumber}, State = {this.State.ToString()}, Amount = {this.Amount}, " + 
                $"Currency = {this.Currency}, PayerEmail = {this.Payer.Customer.Email}"
            );
        }
    }
}
