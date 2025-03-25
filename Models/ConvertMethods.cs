using GoPay.Common;
using GoPay.Model.Payments;
using static GoPay.Model.Payments.Payment;

namespace GoPayCardPayment.Models
{
    /// <summary>
    /// Zde se nachází metody, které konvertují objekty na jiné
    /// </summary>
    public class ConvertMethods
    {
        /// <summary>
        /// Konvertuje string třípísmenný kód měny, na typ goPay Currency (Enum)
        /// </summary>
        /// <param name="currencyStr">string kód měny, který budeme konvertovat</param>
        /// <returns>goPay Currency (Enum)</returns>
        public static Currency ConvertStringToCurrencyEnum(string currencyStr)
        {
            currencyStr = currencyStr.ToLower();
            return currencyStr switch
            {
                "czk" => Currency.CZK,
                "eur" => Currency.EUR,
                "pln" => Currency.PLN,
                "huf" => Currency.HUF,
                "usd" => Currency.USD,
                "gbp" => Currency.GBP,
                "bgn" => Currency.BGN,
                "ron" => Currency.RON,
                _ => Currency.CZK,
            };
        }

        /// <summary>
        /// Konvertuje string třípísmenný kód státu podle standardu ISO 3166-1 alpha-3, na typ goPay Country (Enum)
        /// </summary>
        /// <param name="countryStr">string kód státu, který budeme konvertovat</param>
        /// <returns>goPay Country (Enum)</returns>
        public static Country ConvertStrCountryToGoPayCountry(string countryStr)
        {
            countryStr = countryStr.ToLower();
            return countryStr switch
            {
                "cze" => Country.CZE,
                "svk" => Country.SVK,
                "pol" => Country.POL,
                "deu" => Country.DEU,
                "aut" => Country.AUT,
                "hun" => Country.HUN,
                "gbr" => Country.GBR,
                "usa" => Country.USA,
                _ => Country.CZE,
            };
        }

        /// <summary>
        /// Konvertuje string identifikující stav platby učiněné přes goPay, na typ goPay SessionState (Enum)
        /// </summary>
        /// <param name="stateStr">string stav platby, který budeme konvertovat</param>
        /// <returns>goPay SessionState (Enum)</returns>
        public static SessionState ConvertStrStateToNotificationState(string stateStr)
        {
            stateStr = stateStr.ToLower();

            return stateStr switch
            {
                "created" => SessionState.CREATED,                                  // Platba vytvořena
                "paid" => SessionState.PAID,                                        // Platba uhrazena
                "canceled" => SessionState.CANCELED,                                // Platba zamítnuta
                "payment_method_chosen" => SessionState.PAYMENT_METHOD_CHOSEN,      // Platební metoda potvrzena
                "timeouted" => SessionState.TIMEOUTED,                              // Platbě vypršela životnost
                "autorized" => SessionState.AUTHORIZED,                             // Platba předautorizována
                "refunded" => SessionState.REFUNDED,                                // Platba vrácena
                "partially_refunded" => SessionState.PARTIALLY_REFUNDED,            // Platba částečně vrácena
                _ => SessionState.CANCELED,
            };
        }

        /// <summary>
        /// Konvertuje string identifikující možné platební metody, přes které učiníme goPay transakci, na typ goPay PaymentInstrument (Enum)
        /// </summary>
        /// <param name="paymentInstrumentStr">string identifikující platební metodu, který budeme konvertovat</param>
        /// <returns>goPay PaymentInstrument (Enum)</returns>
        public static PaymentInstrument ConvertStrPaymentInstrumentToGoPayPaymentInstrument(string paymentInstrumentStr)
        {
            paymentInstrumentStr = paymentInstrumentStr.ToLower();
            return paymentInstrumentStr switch
            {
                "payment_card" => PaymentInstrument.PAYMENT_CARD,                       // Platební karta
                "bank_account" => PaymentInstrument.BANK_ACCOUNT,                       // Bankovní převod
                "gpay" => PaymentInstrument.GPAY,                                       // Google Pay
                "apple_pay" => PaymentInstrument.APPLE_PAY,                             // Apple Pay
                "paypal" => PaymentInstrument.PAYPAL,                                   // PayPal účet
                "mpayment" => PaymentInstrument.MPAYMENT,                               // mPlatba (mobilní platba)
                "prsms" => PaymentInstrument.PRSMS,                                     // Premium SMS
                "paysafecard" => PaymentInstrument.PAYSAFECARD,                         // PaySafeCard kupón
                "bitcoin" => PaymentInstrument.BITCOIN,                                 // Bitcoin peněženka
                "click_to_pay" => PaymentInstrument.CLICK_TO_PAY,                       // Click To Pay VISA/MasterCard
                _ => PaymentInstrument.PAYMENT_CARD,
            };
        }

        /// <summary>
        /// Konvertuje goPayPayment (= goPay objekt reprezentující platbu) na eshopPayment (= objekt podobný goPayPayment, s úpravami pro potřeby 
        /// této aplikace (např. primární klíče pro ukládání do SQL))
        /// + daný objekt validuje (např. ověří, zda zákazník, který platbu učinil je uložen v databázi (pokud ne - vrátí null), 
        /// vyhledá primární klíče pro payment_card, která byla použita a payer (pokud nejsou v databázi uloženy, nastaví je)
        /// a payer (pokud nejsou v SQL, PK nastaví na null a vloží se do něj později))
        /// </summary>
        /// <param name="goPayPayment">goPay objekt, který se bude konvertovat na eshopPayment</param>
        /// <returns>
        ///     null, pokud zákazník, který platbu učinil, není uložen v databázi,
        ///         , jinak vrátí eshopPayment reprezentující goPay platbu
        /// </returns>
        public async static Task<EshopPayment?> ConvertGoPayPaymentToEshopPayment(Payment goPayPayment)
        {
            long paymentID = goPayPayment.Id;
            string orderNumber = goPayPayment.OrderNumber;
            Payment.SessionState state = goPayPayment.State ?? Payment.SessionState.CANCELED;
            PaymentInstrument paymentInstrument = goPayPayment.PaymentInstrument ?? PaymentInstrument.PAYMENT_CARD;

            // Celková částka je u GoPay v haléřích/centech převedeme ji na celé koruny/eura, atd.
            decimal amount = (decimal)goPayPayment.Amount / 100m;

            Currency currency = goPayPayment.Currency;

            string lang = goPayPayment.Lang;
            string gwURL = goPayPayment.GwUrl;

            PayerContact contact = goPayPayment.Payer.Contact;
            PayerPaymentCard goPayPaymentCard = goPayPayment.Payer.PaymentCard;

            // Hledání primárního klíče zákazníka, který vykonal platbu přes goPay
            int? customerPK = await PGSQL_Handler.GetCustomerPKByOtherAttributes(
                contact.FirstName, contact.LastName, contact.Email, contact.PhoneNumber,
                contact.City, contact.Street, contact.PostalCode, contact.CountryCode ?? Country.CZE
            );

            Customer customer;

            // Plátce (zákazník + platební karta) byl nalezen:
            if (customerPK != null)
            {
                customer = new Customer
                {
                    CustomerPK = customerPK ?? 0,
                    FirstName = contact.FirstName,
                    LastName = contact.LastName,
                    Email = contact.Email,
                    PhoneNumber = contact.PhoneNumber,
                    City = contact.City,
                    Street = contact.Street,
                    PostalCode = contact.PostalCode,
                    CountryCode = contact.CountryCode ?? Country.CZE
                };
            }
            // Plátce nebyl v SQL nalezen
            else
            {
                Console.Error.WriteLine($"Zákazník, který platbu učinil ({contact.FirstName}, {contact.LastName}, {contact.Email}) nebyl nalezen v databázi zákazníků");
                return null;
            }

            // Zjištění, zda je použitá platební karta již v SQL...
            // paymentCardPK == null -> tato platební karta není uložená v SQL -> musíme ji do ní uložit
            int? nullablePaymentCardPK = await PGSQL_Handler.GetPaymentCardPKByOtherAttributes(
                goPayPaymentCard.CardNumber, goPayPaymentCard.CardExpiration, goPayPaymentCard.CardBrand,
                ConvertStrCountryToGoPayCountry(goPayPaymentCard.CardIssuerCountry), goPayPaymentCard.CardIssuerBank, goPayPaymentCard.CardFingerPrint
            );

            int? paymentCardPK = nullablePaymentCardPK.HasValue ? nullablePaymentCardPK.Value : null;

            PaymentCard paymentCard = new PaymentCard
            {
                paymentCardPK = paymentCardPK,
                cardNumber = goPayPaymentCard.CardNumber,
                cardExpiration = goPayPaymentCard.CardExpiration,
                cardBrand = goPayPaymentCard.CardBrand,
                cardIssuerCountry = ConvertStrCountryToGoPayCountry(goPayPaymentCard.CardIssuerCountry),
                cardIssuerBank = goPayPaymentCard.CardIssuerBank
            };

            // Zjištění, zda je tento payer již uložen v SQL...
            // payerPK == null -> tento plátce není v v SQL -> musíme ji do ní uložit
            int? nullablePayerPK = null;
            int? payerPK = null;

            if (paymentCardPK != null && customerPK != null)
            {
                nullablePayerPK = await PGSQL_Handler.GetPayerPKByOtherAttributes(paymentCardPK.Value, customerPK.Value);

                payerPK = nullablePayerPK.HasValue ? nullablePayerPK.Value : null;

            }

            EshopPayer payer = new EshopPayer { PayerPK = payerPK, PaymentCard = paymentCard, Customer = customer };

            int? nullablePaymentPK;
            int? paymentPK = null;

            // Zjistíme, jestli tato platba není již uložena v SQL
            if (customerPK != null && paymentCardPK != null && payer.PayerPK != null)
            {
                nullablePaymentPK = await PGSQL_Handler.GetPaymentPKByOtherAttributes(
                        paymentID, orderNumber, state, paymentInstrument, amount, currency, payer.PayerPK.Value, lang, gwURL
                    );
                paymentPK = nullablePaymentPK.HasValue ? nullablePaymentPK.Value : null;

            }

            EshopPayment result = new EshopPayment
            {
                PaymentPK = paymentPK,
                PaymentID = paymentID,
                OrderNumber = orderNumber,
                State = state,
                PaymentInstrument = paymentInstrument,
                Amount = amount,
                Currency = currency,
                Payer = payer,
                Lang = goPayPayment.Lang,
                GwURL = goPayPayment.GwUrl
            };

            return result;
        }
    }
}
