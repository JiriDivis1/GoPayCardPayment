namespace GoPayCardPayment.Models
{
    // Když dojde k chybě, bude aplikace přesměrovaná na errorPage, tento objekt obsahuje data potřebná pro 
    public class ErrorObject
    {
        public string errorMessage { get; set; } = string.Empty;
    }
}
