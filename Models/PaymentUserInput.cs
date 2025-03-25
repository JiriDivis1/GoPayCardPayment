namespace GoPayCardPayment.Models
{
    /// <summary>
    /// Formát dat, který je posílán při odeslání formuláře v CreatePaymentView, 
    /// kde vybereme zákazníka, který učiní platbu přes GoPay, částku a měnu
    /// </summary>
    public class PaymentUserInput
    {
        private static readonly string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        public string Amount { get; set; } = string.Empty;              // Celková částka platby v háléřích/centech
        public string Currency { get; set; } = string.Empty;            // Měna platby

        // Má se jednat o identifikace objednávky v rámci eshopu, prozatím je to řetězec náhodných znaků (délky 6), generovaných v metodě níže
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerID { get; set; } = string.Empty;          // ID zákazníka, který platbu uskuteční

        /// <summary>
        /// do atributu orderNumber nastaví náhodný řetězec znaků o délce 6
        /// </summary>
        public void GenerateOrderNumber()
        {
            int randStrLength = 6;
            Random random = new();
            char[] randomString = new char[randStrLength];

            for (int i = 0; i < randStrLength; i++)
            {
                randomString[i] = validChars[random.Next(validChars.Length)];
            }

            this.OrderNumber = new string(randomString);
        }

        public override string ToString()
        {
            return (
                $"amount = {this.Amount}, currency = {this.Currency}, OrderNumber = {this.OrderNumber}, CustomerID = {this.CustomerID}\n"
            );
        }
    }
}
