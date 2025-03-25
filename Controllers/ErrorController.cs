using GoPayCardPayment.Models;
using Microsoft.AspNetCore.Mvc;

namespace GoPayCardPayment.Controllers
{
    public class ErrorController : Controller
    {

        [Route("error")]
        public IActionResult Error(string message)
        {
            ErrorObject errorObj = new ErrorObject { errorMessage = message };
            return View(errorObj);
        }
    }
}
