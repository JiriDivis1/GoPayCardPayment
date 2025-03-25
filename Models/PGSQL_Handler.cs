using GoPay.Common;
using GoPay.Model.Payments;
using static GoPay.Model.Payments.Payment;
using Npgsql;

namespace GoPayCardPayment.Models
{
    /// <summary>
    /// Tento handler obsahuje API pro ukládání/získávání dat do/z PostgreSQL databáze
    /// </summary>
    public class PGSQL_Handler
    {
        /// <returns>Vrátí seznam zákazníků, který získá z SQL tabulky customer, při chybě vrátí null </returns>
        public static List<Customer>? GetCustomersFromPostgres()
        {
            List<Customer> result = [];
            try
            {

                NpgsqlConnection connection = new();

                // Existuje connectionString?
                if (GlobalVariables.connectionString == string.Empty)
                {
                    Console.Error.WriteLine($"Connection string není dostupný");
                    return null;
                }

                connection.ConnectionString = GlobalVariables.connectionString;
                connection.Open();

                NpgsqlCommand sqlSelect = new("SELECT * FROM customer ORDER BY customer_pk;", connection);
                using (var reader = sqlSelect.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Customer customer = new()
                        {
                            CustomerPK = reader.GetInt32(reader.GetOrdinal("customer_pk")),
                            FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                            LastName = reader.GetString(reader.GetOrdinal("last_name")),
                            Email = reader.GetString(reader.GetOrdinal("email")),
                            PhoneNumber = reader.GetString(reader.GetOrdinal("phone_number")),
                            City = reader.GetString(reader.GetOrdinal("city")),
                            Street = reader.GetString(reader.GetOrdinal("street")),
                            PostalCode = reader.GetString(reader.GetOrdinal("postal_code")),
                            CountryCode = ConvertMethods.ConvertStrCountryToGoPayCountry(reader.GetString(reader.GetOrdinal("country_code")))
                        };
                            
                        result.Add(customer);
                            
                    }
                }

                connection.Close();
            }
            catch (NpgsqlException ex)
            {
                Console.Error.WriteLine($"Chyba při získávání seznamu zákazníků: {ex.Message}");
                return null;
            }
            
            return result;
        }

        /// <param name="customerPK"> Primární klíč zákazníka, jehož atributy chceme z SQL získat</param>
        /// <returns>Vrátí z SQL zákazníka, podle primárního klíče</returns>
        public static async Task<Customer?> GetCustomerByPK(int customerPK)
        {
            if (GlobalVariables.connectionString == string.Empty)
            {
                return null;
            }
            try
            {
                await using NpgsqlConnection connection = new(GlobalVariables.connectionString);
                await connection.OpenAsync();

                NpgsqlCommand sqlSelect = new("SELECT * FROM customer WHERE customer_pk = @customerPK;", connection);
                sqlSelect.Parameters.AddWithValue("@customerPK", customerPK);

                await using var reader = await sqlSelect.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new Customer
                    {
                        CustomerPK = customerPK,
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        Email = reader.GetString(3),
                        PhoneNumber = reader.GetString(4),
                        City = reader.GetString(5),
                        Street = reader.GetString(6),
                        PostalCode = reader.GetString(7),
                        CountryCode = ConvertMethods.ConvertStrCountryToGoPayCountry(reader.GetString(8))
                    };
                }
            }
            catch (NpgsqlException ex)
            {
                Console.Error.WriteLine($"Chyba při připojování k databázi (getCustomerByPK): {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Nastala chyba (v metodě getCustomerByPK): {ex.Message}");
            }

            return null;
        }

        /// <returns>Vrátí Primární klíč zákazníka, podle ostatních atributů</returns>
        public async static Task<int?> GetCustomerPKByOtherAttributes(string firstName, string lastName, string email, string phoneNumber, string city, string street, string postalCode, Country countryCode)
        {
            if (GlobalVariables.connectionString == string.Empty)
            {
                return null;
            }
            try
            {
                await using NpgsqlConnection connection = new(GlobalVariables.connectionString);
                await connection.OpenAsync();

                string sqlQuery = (
                    "SELECT customer_pk FROM customer WHERE " +
                    "first_name = @firstName AND last_name = @lastName AND email = @email AND phone_number = @phoneNumber AND " +
                    "city = @city AND street = @street AND postal_code = @postalCode AND country_code = @countryCode::\"Country_code\"" +
                    ";"
                );

                NpgsqlCommand sqlSelect = new(sqlQuery, connection);

                sqlSelect.Parameters.AddWithValue("@firstName", firstName);
                sqlSelect.Parameters.AddWithValue("@lastName", lastName);
                sqlSelect.Parameters.AddWithValue("@email", email);
                sqlSelect.Parameters.AddWithValue("@phoneNumber", phoneNumber);
                sqlSelect.Parameters.AddWithValue("@city", city);
                sqlSelect.Parameters.AddWithValue("@street", street);
                sqlSelect.Parameters.AddWithValue("@postalCode", postalCode);
                sqlSelect.Parameters.AddWithValue("@countryCode", countryCode.ToString());

                await using var reader = await sqlSelect.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return reader.IsDBNull(reader.GetOrdinal("customer_pk")) ? null : reader.GetInt32(reader.GetOrdinal("customer_pk"));
                }
            }
            catch (NpgsqlException ex)
            {
                Console.Error.WriteLine($"Chyba při připojování k databázi: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Nastala chyba (v metodě getCustomerIDByOtherAttributes): {ex.Message}");
            }

            return null;
        }

        /// <returns>Vrátí Primární klíč platební karty (z tabulky payment_card), podle ostatních atributů</returns>
        public async static Task<int?> GetPaymentCardPKByOtherAttributes(string cardNumber, string cardExpiration, string cardBrand, Country cardIssuerCountry, string cardIssuerBank, string? cardFingerprint)
        {
            if (GlobalVariables.connectionString == string.Empty)
            {
                return null;
            }
            try
            {
                await using NpgsqlConnection connection = new(GlobalVariables.connectionString);
                await connection.OpenAsync();

                string sqlQuery = (
                    "SELECT payment_card_pk FROM payment_card WHERE " +
                    "card_number = @cardNumber AND " +
                    "card_expiration = @cardExpiration AND " +
                    "card_brand = @cardBrand AND " +
                    "card_issuer_country = @cardIssuerCountry::\"Country_code\" AND " +
                    "card_issuer_bank = @cardIssuerBank AND " +
                    "card_fingerprint = @cardFingerprint" +
                    ";"
                );

                NpgsqlCommand sqlSelect = new(sqlQuery, connection);

                var countryCodeStr = cardIssuerCountry.ToString();

                sqlSelect.Parameters.AddWithValue("@cardNumber", cardNumber);
                sqlSelect.Parameters.AddWithValue("@cardExpiration", cardExpiration);
                sqlSelect.Parameters.AddWithValue("@cardBrand", cardBrand);
                sqlSelect.Parameters.AddWithValue("@cardIssuerCountry", countryCodeStr);
                sqlSelect.Parameters.AddWithValue("@cardIssuerBank", cardIssuerBank);
                sqlSelect.Parameters.AddWithValue("@cardFingerprint", cardFingerprint ?? "");

                await using var reader = await sqlSelect.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return reader.IsDBNull(reader.GetOrdinal("payment_card_pk")) ? null : reader.GetInt32(reader.GetOrdinal("payment_card_pk"));
                }
            }
            catch (NpgsqlException ex)
            {
                Console.Error.WriteLine($"Chyba při připojování k databázi: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Nastala chyba (v metodě getPaymentCardByOtherAttributes): {ex.Message}");
            }

            return null;
        }

        /// <returns>Vrátí Primární klíč plátce (z tabulky payer), podle ostatních atributů</returns>
        public async static Task<int?> GetPayerPKByOtherAttributes(int paymentCardFK, int customerFK)
        {
            if (GlobalVariables.connectionString == string.Empty)
            {
                return null;
            }
            try
            {
                await using NpgsqlConnection connection = new(GlobalVariables.connectionString);
                await connection.OpenAsync();

                string sqlQuery = (
                    "SELECT payer_pk FROM payer WHERE " +
                    "payment_card_fk = @paymentCardFK AND " +
                    "customer_fk = @customerFK" +
                    ";"
                );

                NpgsqlCommand sqlSelect = new(sqlQuery, connection);

                sqlSelect.Parameters.AddWithValue("@paymentCardFK", paymentCardFK);
                sqlSelect.Parameters.AddWithValue("@customerFK", customerFK);

                await using var reader = await sqlSelect.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return reader.IsDBNull(reader.GetOrdinal("payer_pk")) ? null : reader.GetInt32(reader.GetOrdinal("payer_pk"));
                }
            }
            catch (NpgsqlException ex)
            {
                Console.Error.WriteLine($"Chyba při připojování k databázi: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Nastala chyba (v metodě getPayerPKByOtherAttributes): {ex.Message}");
            }

            return null;
        }

        /// <returns>Vrátí Primární klíč platby (z tabulky payment), podle ostatních atributů</returns>
        public async static Task<int?> GetPaymentPKByOtherAttributes(
            long paymentID, string orderNumber, SessionState state, PaymentInstrument paymentInstrument, 
            decimal amount, Currency currency, int payerFK, string lang, string gwURL)
        {
            if (GlobalVariables.connectionString == string.Empty)
            {
                return null;
            }
            try
            {
                await using NpgsqlConnection connection = new(GlobalVariables.connectionString);
                await connection.OpenAsync();

                string sqlQuery = (
                    "SELECT payment_pk FROM payment WHERE " +
                    "payment_id = @paymentID AND " +
                    "order_number = @orderNumber AND " +
                    "state = @state::\"State\" AND " +
                    "payment_instrument = @paymentInstrument::\"Payment_instrument\" AND " +
                    "amount = @amount AND " +
                    "currency = @currency::\"Currency\" AND " +
                    "payer_fk = @payerFK AND " +
                    "lang = @lang AND " +
                    "gw_url = @gwURL" +
                    ";"
                );

                NpgsqlCommand sqlSelect = new(sqlQuery, connection);

                sqlSelect.Parameters.AddWithValue("@paymentID", paymentID);
                sqlSelect.Parameters.AddWithValue("@orderNumber", orderNumber);
                sqlSelect.Parameters.AddWithValue("@state", state.ToString());
                sqlSelect.Parameters.AddWithValue("@paymentInstrument", paymentInstrument.ToString());
                sqlSelect.Parameters.AddWithValue("@amount", amount);
                sqlSelect.Parameters.AddWithValue("@currency", currency.ToString());
                sqlSelect.Parameters.AddWithValue("@payerFK", payerFK);
                sqlSelect.Parameters.AddWithValue("@lang", lang);
                sqlSelect.Parameters.AddWithValue("@gwURL", gwURL);

                await using var reader = await sqlSelect.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return reader.IsDBNull(reader.GetOrdinal("payment_pk")) ? null : reader.GetInt32(reader.GetOrdinal("payment_pk"));
                }
            }
            catch (NpgsqlException ex)
            {
                Console.Error.WriteLine($"Chyba při připojování k databázi: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Nastala chyba (v metodě getCustomerIDByOtherAttributes): {ex.Message}");
            }

            return null;
        }

        /// Používá se v PaymentListingView.cshtml
        /// <returns>Vrátí seznam plateb, který získá z SQL tabulky payment, při chybě vrátí null </returns>
        public static List<EshopPayment>? GetPaymentsFromPostgres()
        {
            List<EshopPayment> result = [];
            try
            {
                string sqlQuery = @"
                SELECT
                    payment.payment_pk, payment.payment_id, payment.order_number, payment.state, payment.payment_instrument,
                    payment.amount, payment.currency, payment.lang, payment.gw_url, payer.payer_pk, payer.payment_card_fk,
                    payer.customer_fk, payment_card.card_number, payment_card.card_expiration, payment_card.card_brand,
                    payment_card.card_issuer_country, payment_card.card_issuer_bank, payment_card.card_fingerprint, customer.first_name,
                    customer.last_name, customer.email, customer.phone_number, customer.city, customer.street,
                    customer.postal_code, customer.country_code
                FROM
                    payment
                INNER JOIN payer ON payment.payer_fk = payer.payer_pk
                INNER JOIN payment_card ON payer.payment_card_fk = payment_card.payment_card_pk
                INNER JOIN customer ON payer.customer_fk = customer.customer_pk ORDER BY payment.payment_pk;
            ";

                // Existuje connectionString?
                if (GlobalVariables.connectionString == string.Empty)
                {
                    Console.Error.WriteLine($"Connection string není dostupný");
                    return null;
                }

                NpgsqlConnection connection = new()
                {
                    ConnectionString = GlobalVariables.connectionString
                };
                connection.Open();

                NpgsqlCommand sqlSelect = new(sqlQuery, connection);
                using (var reader = sqlSelect.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Customer customer = new()
                        {
                            CustomerPK = reader.GetInt32(11),
                            FirstName = reader.GetString(18),
                            LastName = reader.GetString(19),
                            Email = reader.GetString(20),
                            PhoneNumber = reader.GetString(21),
                            City = reader.GetString(22),
                            Street = reader.GetString(23),
                            PostalCode = reader.GetString(24),
                            CountryCode = ConvertMethods.ConvertStrCountryToGoPayCountry(reader.GetString(25)),
                        };

                        PaymentCard paymentCard = new()
                        {
                            paymentCardPK = reader.GetInt32(10),
                            cardNumber = reader.GetString(12),
                            cardExpiration = reader.GetString(13),
                            cardBrand = reader.GetString(14),
                            cardIssuerCountry = ConvertMethods.ConvertStrCountryToGoPayCountry(reader.GetString(15)),
                            cardIssuerBank = reader.GetString(16),
                            cardFingerprint = reader.GetString(17) == "" ? null : reader.GetString(17),
                        };

                        EshopPayer payer = new()
                        {
                            PayerPK = reader.GetInt32(9),
                            PaymentCard = paymentCard,
                            Customer = customer,
                        };

                        result.Add(
                            new EshopPayment
                            {
                                PaymentPK = reader.GetInt32(0),
                                PaymentID = reader.GetInt64(1),
                                OrderNumber = reader.GetString(2),
                                State = ConvertMethods.ConvertStrStateToNotificationState(reader.GetString(3)),
                                PaymentInstrument = ConvertMethods.ConvertStrPaymentInstrumentToGoPayPaymentInstrument(reader.GetString(4)),
                                Amount = reader.GetDecimal(5),
                                Currency = ConvertMethods.ConvertStringToCurrencyEnum(reader.GetString(6)),
                                Payer = payer,
                                Lang = reader.GetString(7),
                                GwURL = reader.GetString(8)
                            }
                        );
                    }
                }

                connection.Close();
            }
            catch (NpgsqlException ex)
            {
                Console.Error.WriteLine($"Chyba při získávání pole zákazníků: {ex.Message}");
            }

            return result;
        }

        /// Používá se v RefundationView.cshtml
        /// <returns>Vrátí seznam ZAPLACENÝCH plateb (možnost refundace), který získá z SQL tabulky payment, při chybě vrátí null </returns>
        public static List<EshopPayment>? GetOnlyPaidPaymentsFromPostgres()
        {
            List<EshopPayment> result = [];
            try
            {
                string sqlQuery = @"
                SELECT
                    p1.payment_pk, p1.payment_id, p1.order_number, p1.state, p1.payment_instrument,
                    p1.amount, p1.currency, p1.lang, p1.gw_url, payer.payer_pk, payer.payment_card_fk,
                    payer.customer_fk, payment_card.card_number, payment_card.card_expiration, payment_card.card_brand,
                    payment_card.card_issuer_country, payment_card.card_issuer_bank, payment_card.card_fingerprint, customer.first_name,
                    customer.last_name, customer.email, customer.phone_number, customer.city, customer.street,
                    customer.postal_code, customer.country_code
                FROM
                    payment p1
                INNER JOIN payer ON p1.payer_fk = payer.payer_pk
                INNER JOIN payment_card ON payer.payment_card_fk = payment_card.payment_card_pk
                INNER JOIN customer ON payer.customer_fk = customer.customer_pk
                WHERE p1.state = 'PAID'
                AND NOT EXISTS (
                    SELECT 1            
                    FROM payment p2
                    WHERE p2.payment_id = p1.payment_id
                    AND p2.state = 'REFUNDED'
                );
            ";

                // Existuje connectionString?
                if (GlobalVariables.connectionString == string.Empty)
                {
                    Console.Error.WriteLine($"Connection string není dostupný");
                    return null;
                }

                NpgsqlConnection connection = new NpgsqlConnection
                {
                    ConnectionString = GlobalVariables.connectionString
                };
                connection.Open();

                NpgsqlCommand sqlSelect = new(sqlQuery, connection);
                using (var reader = sqlSelect.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Customer customer = new Customer
                        {
                            CustomerPK = reader.GetInt32(11),
                            FirstName = reader.GetString(18),
                            LastName = reader.GetString(19),
                            Email = reader.GetString(20),
                            PhoneNumber = reader.GetString(21),
                            City = reader.GetString(22),
                            Street = reader.GetString(23),
                            PostalCode = reader.GetString(24),
                            CountryCode = ConvertMethods.ConvertStrCountryToGoPayCountry(reader.GetString(25)),
                        };

                        PaymentCard paymentCard = new()
                        {
                            paymentCardPK = reader.GetInt32(10),
                            cardNumber = reader.GetString(12),
                            cardExpiration = reader.GetString(13),
                            cardBrand = reader.GetString(14),
                            cardIssuerCountry = ConvertMethods.ConvertStrCountryToGoPayCountry(reader.GetString(15)),
                            cardIssuerBank = reader.GetString(16),
                            cardFingerprint = reader.GetString(17) == "" ? null : reader.GetString(17),
                        };

                        EshopPayer payer = new()
                        {
                            PayerPK = reader.GetInt32(9),
                            PaymentCard = paymentCard,
                            Customer = customer,
                        };

                        result.Add(
                            new EshopPayment
                            {
                                PaymentPK = reader.GetInt32(0),
                                PaymentID = reader.GetInt64(1),
                                OrderNumber = reader.GetString(2),
                                State = ConvertMethods.ConvertStrStateToNotificationState(reader.GetString(3)),
                                PaymentInstrument = ConvertMethods.ConvertStrPaymentInstrumentToGoPayPaymentInstrument(reader.GetString(4)),
                                Amount = reader.GetDecimal(5),
                                Currency = ConvertMethods.ConvertStringToCurrencyEnum(reader.GetString(6)),
                                Payer = payer,
                                Lang = reader.GetString(7),
                                GwURL = reader.GetString(8)
                            }
                        );
                    }
                }

                connection.Close();
            }
            catch (NpgsqlException ex)
            {
                Console.Error.WriteLine($"Chyba při získávání pole zákazníků: {ex.Message}");
            }

            return result;
        }

        /// <param name="paymentPK"></param>
        /// <returns>Vrátí ID platby a částku platby podle primárního klíče, tyto 2 atributy potřebujeme pro refundaci</returns>
        public async static Task<RefundationRequest?> GetPaymentID_And_AmountByPK (int paymentPK)
        {
            if (GlobalVariables.connectionString == string.Empty)
            {
                return null;
            }
            try
            {
                await using NpgsqlConnection connection = new(GlobalVariables.connectionString);
                await connection.OpenAsync();

                NpgsqlCommand sqlSelect = new("SELECT payment_id, amount FROM payment WHERE payment_pk = @paymentPK;", connection);
                sqlSelect.Parameters.AddWithValue("@paymentPK", paymentPK);

                await using var reader = await sqlSelect.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new RefundationRequest
                    {
                        PaymentID = reader.GetInt64(0),
                        Amount = reader.GetDecimal(1),
                    };
                }
            }
            catch (NpgsqlException ex)
            {
                Console.Error.WriteLine($"Chyba při připojování k databázi (getCustomerByPK): {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Nastala chyba (v metodě getCustomerByPK): {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Tato metoda vloží údaje o platební kartě do SQL tabulky payment_card
        /// </summary>
        /// <param name="paymentCard">Platební karta, která se uloží do SQL</param>
        /// <param name="connection">Připojení k databázi</param>
        /// <param name="transaction">Tato metoda se má vykonat v této transakci, pokud není null</param>
        /// <returns>Primární klíč nově vložené platební karty, null -> pokud došlo k chybě</returns>
        public async static Task<int?> InsertPaymentCardToPostgres(PaymentCard paymentCard, NpgsqlConnection connection, NpgsqlTransaction? transaction = null)
        {
            string sqlQuery = (
                "INSERT INTO payment_card (" +
                    "card_number, card_expiration, card_brand, card_issuer_country, card_issuer_bank, card_fingerprint" +
                ") VALUES (" +
                    "@cardNumber, @cardExpiration, @cardBrand, @cardIssuerCountry::\"Country_code\", @cardIssuerBank, @cardFingerprint" +
                ") RETURNING payment_card_pk;"
            );

            NpgsqlCommand sqlInsert;
            if (transaction != null)
            {
                sqlInsert = new NpgsqlCommand(sqlQuery, connection, transaction);
            } else {
                sqlInsert = new NpgsqlCommand(sqlQuery, connection);
            }

            sqlInsert.Parameters.AddWithValue("@cardNumber", paymentCard.cardNumber);
            sqlInsert.Parameters.AddWithValue("@cardExpiration", paymentCard.cardExpiration);
            sqlInsert.Parameters.AddWithValue("@cardBrand", paymentCard.cardBrand);
            sqlInsert.Parameters.AddWithValue("@cardIssuerCountry", paymentCard.cardIssuerCountry.ToString());
            sqlInsert.Parameters.AddWithValue("@cardIssuerBank", paymentCard.cardIssuerBank);
            sqlInsert.Parameters.AddWithValue("@cardFingerprint", paymentCard.cardFingerprint ?? "");

            int? result = (int?)await sqlInsert.ExecuteScalarAsync();

            if (result != null) {
                Console.WriteLine("Byl proveden insert do tabulky payment_card.");
            }

            return result;
        }

        /// <summary>
        /// Tato metoda vloží údaje o plátci do SQL tabulky payer
        /// </summary>
        /// <param name="paymentCardFK">Cizí klíč odkazující k platební kartě, kterou plátce použil</param>
        /// <param name="customerFK">Cizí klíč odkazující k údajích o plátci</param>
        /// <param name="connection">Připojení k databázi</param>
        /// <param name="transaction">Tato metoda se má vykonat v této transakci, pokud není null</param>
        /// <returns>Primární klíč nově vloženého plátce, null -> pokud došlo k chybě</returns>
        public async static Task<int?> InsertPayerToPostgres(int paymentCardFK, int customerFK, NpgsqlConnection connection, NpgsqlTransaction? transaction = null)
        {
            string sqlQuery = "INSERT INTO payer (payment_card_fk, customer_fk) VALUES (@paymentCardFK, @customerFK) RETURNING payer_pk;";

            NpgsqlCommand sqlInsert;
            if (transaction != null)
            {
                sqlInsert = new NpgsqlCommand(sqlQuery, connection, transaction);
            }
            else
            {
                sqlInsert = new NpgsqlCommand(sqlQuery, connection);
            }

            sqlInsert.Parameters.AddWithValue("@paymentCardFK", paymentCardFK);
            sqlInsert.Parameters.AddWithValue("@customerFK", customerFK);

            int? result = (int?)await sqlInsert.ExecuteScalarAsync();

            if (result != null)
            {
                Console.WriteLine("Byl proveden insert do tabulky payer.");
            }

            return result;
        }

        /// <summary>
        /// Tato metoda vloží platbu do SQL tabulky payment, pokud plátce a platební karta nejsou v SQL, také je tam vloží
        /// Předpokládá se, že data jsou konzistentní (ověření probíhá v metodě ConvertGoPayPaymentToEshopPayment)
        /// </summary>
        /// <param name="payment">Platba které se má vložit do SQL tabulky payment</param>
        /// <returns>Primární klíč nově vložené platby, null -> pokud došlo k chybě</returns>
        public async static Task<int?> InsertPaymentToPostgres(EshopPayment payment)
        {
            await using NpgsqlConnection connection = new(GlobalVariables.connectionString);
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            // Přesvědčíme se, zda payer, který učinil transakci a platební karta kterou použil jsou již v databázi uložené, nebo je teprve vložíme
            try
            {
                int? nullablePaymentCardPK = null;
                int paymentCardPK;
                // Platební karta, kterou plátce použil není v databázi -> vložíme jí tam
                if (payment.Payer.PaymentCard.paymentCardPK == null)
                {
                    nullablePaymentCardPK = await InsertPaymentCardToPostgres(payment.Payer.PaymentCard, connection, transaction);

                    if (nullablePaymentCardPK.HasValue)
                    {
                        paymentCardPK = nullablePaymentCardPK.Value;
                        payment.Payer.PaymentCard.paymentCardPK = paymentCardPK;
                    } else
                    {
                        await transaction.RollbackAsync();
                        Console.Error.WriteLine($"Nepodařilo se vložit do databáze platební kartu.");
                        return null;
                    }

                    
                } else
                {
                    paymentCardPK = (int)payment.Payer.PaymentCard.paymentCardPK;
                }
                
                int? nullablePayerPK;
                int payerPK;

                int? result;

                // Tento plátce není v databázi -> vložíme jej tam
                if (payment.Payer.PayerPK == null)
                {
                    nullablePayerPK = await InsertPayerToPostgres(paymentCardPK, payment.Payer.Customer.CustomerPK, connection, transaction);

                    if (nullablePayerPK.HasValue)
                    {
                        payerPK = nullablePayerPK.Value;
                        payment.Payer.PayerPK = payerPK;
                    } else
                    {
                        await transaction.RollbackAsync();
                        Console.Error.WriteLine($"Nepodařilo se vložit do databáze plátce.");
                        return null;
                    }

                } else
                {
                    payerPK = payment.Payer.PayerPK.Value;
                }
                
                if (payment.PaymentPK == null)
                {
                    Console.WriteLine("Insert payment");
                    string sqlQuery = (
                    "INSERT INTO payment (" +
                        "payment_id, order_number, state, payment_instrument, amount, currency, payer_fk, lang, gw_url" +
                    ") VALUES (" +
                        "@paymentID, @orderNumber, @state::\"State\", @paymentInstrument::\"Payment_instrument\", @amount, @currency::\"Currency\", @payerFK, @lang, @gwURL) RETURNING payment_pk;"
                    );

                    NpgsqlCommand sqlInsert;

                    sqlInsert = new NpgsqlCommand(sqlQuery, connection, transaction);


                    sqlInsert.Parameters.AddWithValue("@paymentID", payment.PaymentID);
                    sqlInsert.Parameters.AddWithValue("@orderNumber", payment.OrderNumber);
                    sqlInsert.Parameters.AddWithValue("@state", payment.State.ToString());
                    sqlInsert.Parameters.AddWithValue("@paymentInstrument", payment.PaymentInstrument.ToString());
                    sqlInsert.Parameters.AddWithValue("@amount", payment.Amount);
                    sqlInsert.Parameters.AddWithValue("@currency", payment.Currency.ToString());
                    sqlInsert.Parameters.AddWithValue("@payerFK", payerPK);
                    sqlInsert.Parameters.AddWithValue("@lang", payment.Lang);
                    sqlInsert.Parameters.AddWithValue("@gwURL", payment.GwURL);

                    result = (int?)await sqlInsert.ExecuteScalarAsync();

                    if (result != null)
                    {
                        Console.WriteLine("Byl proveden insert do tabulky payment.");
                    }
                } else
                {
                    result = payment.PaymentPK;
                }


                if (transaction != null)
                {
                    await transaction.CommitAsync();
                }

                return result;

                
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.Error.WriteLine($"Chyba při vkládání dat: {ex.Message}");
                return null;
            }
        }
    }
}
