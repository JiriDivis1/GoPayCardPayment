using GoPay;
using GoPay.Common;
using GoPay.Model.Payment;
using GoPay.Model.Payments;
using GoPay.Payments;

namespace GoPayCardPayment.Models
{
    /// <summary>
    /// Tento objekt se stará o veškeré interakce s GoPay API
    /// Konkrétně: Vyvolání platby, dotaz na stav platby a vyvolání refundace
    /// </summary>
    public class GoPayHandler
    {
        /// <summary>
        /// Metoda pro získání goPay connectoru
        /// </summary>
        /// <returns>Vrací goPay connector</returns>
        private static GPConnector GetGoPayConnector()
        {
            string sandboxURL = GlobalVariables.sandboxURL;
            string clientID = GlobalVariables.clientID;
            string clientSecret = GlobalVariables.clientSecret;

            GPConnector result = new(
                sandboxURL,
                clientID,
                clientSecret
            );

            result.GetAppToken();

            return result;
        }

        /// <summary>
        /// Vyvolá platbu přes GoPay a vrací (v případě úspěchu URL platební brány)
        /// </summary>
        /// <param name="amount">Celková částka platby v celých korunách/eurech/...</param>
        /// <param name="currency">Měna platby</param>
        /// <param name="orderNumber">Identifikace objednávky v rámci e-shopu</param>
        /// <param name="payer">Data o plátci</param>
        /// <returns>bool indikující, zda vyvolání platby proběhlo úspěšně
        ///         -> true, na druhém místě vrací URL platební brány
        ///         -> false, na druhém místě vrací zprávu o chybě
        /// </returns>
        public static (bool success, string messageOrGwURL) CreateGoPayPayment(long amount, Currency currency, string orderNumber, Customer payer)
        {
            try
            {
                GPConnector goPayConnector = GetGoPayConnector();

                string goID = GlobalVariables.goID;
                string returnURL = GlobalVariables.returnURL;
                string notificationURL = GlobalVariables.notificationURL;

                // Vytvoření platby
                BasePayment payment = new()
                {
                    Amount = amount,
                    Currency = currency,
                    OrderNumber = orderNumber,
                    Payer = new Payer
                    {
                        AllowedPaymentInstruments = [PaymentInstrument.PAYMENT_CARD],
                        Contact = new PayerContact
                        {
                            FirstName = payer.FirstName,
                            LastName = payer.LastName,
                            Email = payer.Email,
                            PhoneNumber = payer.PhoneNumber,
                            City = payer.City,
                            Street = payer.Street,
                            PostalCode = payer.PostalCode,
                            CountryCode = payer.CountryCode,
                        }
                    },
                    Target = new Target
                    {
                        GoId = long.Parse(goID ?? "0"),
                        Type = Target.TargetType.ACCOUNT
                    },
                    Callback = new Callback
                    {
                        ReturnUrl = returnURL,
                        NotificationUrl = notificationURL
                    }
                };

                Payment? result = goPayConnector.CreatePayment(payment);

                if (result != null)
                {
                    string gwURL = result.GwUrl;

                    // Pokud získáme validní URL, přesměrujeme na ni
                    if (!string.IsNullOrEmpty(gwURL))
                    {
                        return (true, gwURL);
                    }
                }
            }
            catch (GoPay.GPClientException ex)
            {
                Console.Error.WriteLine($"Chyba (CreateGoPayPayment): {ex.Message}");
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Chyba (CreateGoPayPayment): {ex.Message}");
                return (false, ex.Message);
            }

            Console.Error.WriteLine($"Chyba: Platba nebyla vytvořena, chyba pravděpodobně v parametrech, které byly goPay předány.");
            return (false, "Platba nebyla vytvořena, chyba pravděpodobně v parametrech, které byly goPay předány.");
        }

        public static async Task<EshopPayment?> StateOfPaymentQuery(long paymentID)
        {
            try
            {
                GPConnector goPayConnector = GetGoPayConnector();

                Payment? paymentStatus = goPayConnector.PaymentStatus(paymentID);
                if (paymentStatus != null)
                {
                    EshopPayment? result = await ConvertMethods.ConvertGoPayPaymentToEshopPayment(paymentStatus);

                    if (result != null)
                    {
                        return result;
                    }
                }

            }
            catch (GoPay.GPClientException ex)
            {
                Console.Error.WriteLine($"Chyba (StateOfPaymentQuery): {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Chyba (StateOfPaymentQuery): {ex.Message}");
            }

            return null;
        }

        public static PaymentResult? CallRefundPayment(long paymentID, long amountInCents)
        {
            try
            {
                GPConnector goPayConnector = GetGoPayConnector();

                return goPayConnector.RefundPayment(paymentID, amountInCents);

            }
            catch (GoPay.GPClientException ex)
            {
                Console.Error.WriteLine($"Chyba (CallRefundation): {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Chyba (CallRefundation): {ex.Message}");
            }

            return null;
        }
    }
}
