using GoPayCardPayment.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GoPayCardPayment.Controllers
{
    public class HomeController : Controller
    {
        private readonly NgrokService _ngrokService;

        public HomeController()
        {
            _ngrokService = new NgrokService();
        }

        public IActionResult Index()
        {

            return View();
        }

        public IActionResult Notification()
        {
            return View();
        }

        // NGROK èást
        // Pøi spuštìní, se aplikace pøesmìru z lokální URL na NGROK
        [HttpGet("get-ngrok-url")]
        public async Task<IActionResult> GetNgrokUrl()
        {
            string currentURL = $"{Request.Scheme}://{Request.Host}";

            if (GlobalVariables.usingNGROK)
            {
                string? ngrokURL = await _ngrokService.GetNgrokUrlAsync();

                if (ngrokURL != null)
                {
                    string newNotificationURL = ngrokURL + "/gopay/notification";

                    GlobalVariables.returnURL = newNotificationURL;
                    GlobalVariables.notificationURL = newNotificationURL;

                    return Json(new { url = ngrokURL });
                } else
                {
                    Console.WriteLine("WARNING: usingNGROK == true, ale nepodaøilo se získat veøejnou NGROK URL, pokud není správnì nastavena returningURL a notificationURL, interakce s GoPay nebude fungovat.\n");
                }
            }

            return NoContent();

        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
