using Microsoft.AspNetCore.Mvc;

namespace My.WebSocket.Client.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
