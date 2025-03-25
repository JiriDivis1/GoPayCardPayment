namespace GoPayCardPayment.Models
{
    /// <summary>
    /// Model obsahující data potřebná pro view k vyvolání platby
    /// -> obsahuje seznam zákazníků, ze kterých bude vybrán jeden, kterů platbu uskuteční
    /// </summary>
    public class CreatePaymentModelView
    {
        public List<Customer> CustomerList { get; set; } = [];
    }
}
