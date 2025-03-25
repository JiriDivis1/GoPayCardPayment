using GoPay;
using GoPay.Model.Payments;
using GoPay.Payments;
using GoPayCardPayment.Models;
using Microsoft.AspNetCore.Mvc;

namespace GoPayCardPayment.Controllers
{
    public class RefundationController : Controller
    {
        [Route("/gopay/refundation")]
        public IActionResult RefundationView()
        {
            // Získání zaplacených plateb:
            List<EshopPayment>? paymentList = PGSQL_Handler.GetOnlyPaidPaymentsFromPostgres();

            if (paymentList == null)
            {
                return RedirectToAction("Error", "Error", new { message = $"Nepodařilo se načíst seznam zaplacených transakcí." });
            }
            else
            {
                return View(new PaymentListingModelView { PaymentList = paymentList });
            }
        }

        public async Task<IActionResult> RefundationBackend(string paymentPKstr)
        {
            int paymentPK = int.Parse(paymentPKstr);

            RefundationRequest? refundationRequest = await PGSQL_Handler.GetPaymentID_And_AmountByPK(paymentPK);

            if (refundationRequest == null)
            {
                return RedirectToAction("Error", "Error", new { message = $"Platba kterou chcete refundovat, nebyla v SQL nalezena, nelze ji tedy refundovat" });
            }
            else
            {
                
                string message = string.Empty;

                long amountInCents = (long)refundationRequest.Amount * 100;

                PaymentResult? result = GoPayHandler.CallRefundPayment(refundationRequest.PaymentID, amountInCents);

                if (result != null && result.Result == PaymentResult.PaymentResults.FINISHED)
                {
                    EshopPayment? payment = await GoPayHandler.StateOfPaymentQuery(refundationRequest.PaymentID);

                    // Dotaz na stav refundované platby selhal
                    if (payment == null)
                    {
                        message = "Vyvolání refundace proběhlo úspěšně, ale nepodařilo se dotázat na stav refundované platby";
                    }
                    // Získání dozazu na stav refundované platby
                    else
                    {
                        // Vložení záznamu o refundované platbě do Postgres
                        int? insertPaymentResult = await PGSQL_Handler.InsertPaymentToPostgres(payment);

                        if (insertPaymentResult.HasValue)
                        {
                            return View("RefundationResult", new SuccessObject { Success = true, Message = "Refundace proběhla úspěšně, záznam o ní byl vložen do Postgres" });
                        }
                        message = "Platba byla refundovaná, ale záznam o refundaci se nepodařilo vložit do Postges.";
                    }
                } else
                {
                    message = "Chyba nastala při vyvolání refundace.";
                }

                return View("RefundationResult", new SuccessObject{ Success = false, Message = message});
            }
        }
    }
}
