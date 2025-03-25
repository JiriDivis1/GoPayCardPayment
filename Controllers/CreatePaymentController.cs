using GoPayCardPayment.Models;
using GoPay;
using Microsoft.AspNetCore.Mvc;
using GoPay.Common;
using System.Security.Policy;

namespace GoPayCardPayment.Controllers
{
    public class CreatePaymentController : Controller
    {
        [Route("gopay/createPayment")]
        public IActionResult CreatePaymentView()
        {
            // Načtení zákazníků z SQL tabulky
            List<Customer>? customerList = PGSQL_Handler.GetCustomersFromPostgres();

            if (customerList == null)
            {
                return RedirectToAction("Error", "Error", new { message = $"Nepodařilo se načíst zákazníky." });
            }

            CreatePaymentModelView model = new()
            {
                CustomerList = customerList
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentBackend(PaymentUserInput paymentUserInput)
        {
            // vygenerování orderNumber (= Identifikace objednávky v rámci e-shopu, alfanumerické znaky)
            paymentUserInput.GenerateOrderNumber();

            // Konvertování string Amount na long Amount:
            long amount = (long)(decimal.Parse(paymentUserInput.Amount.Replace('.', ',')) * 100);    // goPay přímá částku v haléřích/centech, proto * 100
            Currency currency = ConvertMethods.ConvertStringToCurrencyEnum(paymentUserInput.Currency);

            // Získání dat o plátci z SQL tabulku Customer
            Customer? payer = await PGSQL_Handler.GetCustomerByPK(int.Parse(paymentUserInput.CustomerID));

            if (payer == null)
            {
                return RedirectToAction("Error", "Error", new { message = $"Zákazník, který má učinit platbu (ID zákazníka = {paymentUserInput.CustomerID}), nebyl v databázi nalezen." });
            }

            (bool success, string messageOrGwURL) = GoPayHandler.CreateGoPayPayment(amount, currency, paymentUserInput.OrderNumber, payer);

            if (!success)
            {
                return RedirectToAction("Error", "Error", new { message = messageOrGwURL });
            //Přesměrování na URL platební brány
            } else
            {
                return Redirect(messageOrGwURL);
            }
        }
    }
}
