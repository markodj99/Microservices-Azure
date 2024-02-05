using Common.Models.User;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Common.Interfaces;

namespace ClientStateless.Controllers
{
    public class UserController : Controller
    {
        private readonly IApiGateway _proxy 
            = ServiceProxy.Create<IApiGateway>(new Uri("fabric:/Cloud-Project/ApiGatewayStateless"));

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(Login credentials)
        {
            try
            {
                if (await _proxy.LoginAsync(credentials))
                {
                    HttpContext.Session.SetString("Email", credentials.Email);
                    return RedirectToAction("Index", "Home");
                }

                return View();
            }
            catch (Exception)
            {
                return View();
            }       
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Email");
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(Register credentials)
        {
            try
            {
                if (await _proxy.RegisterAsync(credentials)) return RedirectToAction("User", "Login");

                return View();
            }
            catch (Exception)
            {
                return View();
            }
        }
    }
}
