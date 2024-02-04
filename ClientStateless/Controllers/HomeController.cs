using ClientStateless.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ClientStateless.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View(model: HttpContext.Session.GetString("Email"));

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
         =>  View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}