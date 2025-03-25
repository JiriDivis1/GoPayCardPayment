using GoPay;
using GoPay.Model.Payments;
using GoPayCardPayment.Models;
using Microsoft.AspNetCore.Mvc;

namespace GoPayCardPayment.Controllers
{
    public class NotificationController : Controller
    {
        // [FromQuery] = Controller tyto atributy získá z urlQuery (urlParam)
        // Endpoint pro zachytávání notifikace od GoPay
        [HttpGet("gopay/notification")]
        [Route("/gopay/notificationDetails")]
        public async Task<IActionResult> NotificationBackend([FromQuery] string id, [FromQuery] string parentID)
        {
            // Zkontrolujeme, jestli jsme skutečně zachytily ID platby
            if (string.IsNullOrEmpty(id))
            {
                return View("NotificationResult");
            }

            string dateTime = DateTime.Now.ToString();
            Console.WriteLine($"čas = {dateTime}");

            EshopPayment? payment = await GoPayHandler.StateOfPaymentQuery(long.Parse(id));

            if (payment != null)
            {
                int? insertPaymentResult = await PGSQL_Handler.InsertPaymentToPostgres(payment);

                if (insertPaymentResult.HasValue)
                {
                    return View("NotificationResult", payment);
                }

                return RedirectToAction("Error", "Error", new { message = $"Platbu se nepodařilo do SQL vložit." });
            } else
            {
                return View("NotificationResult");
            }
        }

        [Route("/gopay/paymentListing")]
        public IActionResult PaymentListingView()
        {
            List<EshopPayment>? paymentList = PGSQL_Handler.GetPaymentsFromPostgres();

            if (paymentList == null)
            {
                return RedirectToAction("Error", "Error", new { message = $"Nepodařilo se načíst seznam nofifikací." });
            } else
            {
                return View(new PaymentListingModelView { PaymentList = paymentList });
            }


        }
    }
}
