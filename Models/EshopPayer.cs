using GoPay.Model.Payments;

namespace GoPayCardPayment.Models
{
    /// <summary>
    /// Upravená verze objektu GoPay.Model.Payments.Payer, reprezentující plátce, který učinil platbu přes GoPay
    /// </summary>
    public class EshopPayer
    {
        public int? PayerPK { get; set; }                                   // Primární klíč plátce v Postgres tabulce payer, null -> ještě není uložen, nebo PK ještě není zjištěn
        public PaymentCard PaymentCard { get; set; } = new PaymentCard();   // Platební karta, kterou použil
        public Customer Customer { get; set; } = new Customer();            // Údaje o zákazníkovi
    }
}
