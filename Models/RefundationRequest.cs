namespace GoPayCardPayment.Models
{
    public class RefundationRequest
    {
        public long PaymentID { get; set; }     // ID platby
        public decimal Amount { get; set; }      // Částka platby v celých korunách/eurech/...
    }
}
