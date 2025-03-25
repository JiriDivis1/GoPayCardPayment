namespace GoPayCardPayment.Models
{
    /// <summary>
    /// Model obsahující data potřebná pro view k zobrazení existujících plateb (PaymentListing.cshtml a Refundation.cshtml)
    /// -> obsahuje seznam plateb, které se budou zobrazovat v PaymentListing.cshtml
    /// -> a seznam plateb které mohou být refundovány v Refundation.cshtml
    /// </summary>
    public class PaymentListingModelView
    {
        public List<EshopPayment> PaymentList { get; set; } = [];
    }
}
