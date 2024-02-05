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
                    TempData["Success"] = "Login Successful.";
                    return RedirectToAction("Index", "Home");
                }

                TempData["Error"] = "Wrong email or password.";
                return View();
            }
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong, please try again later.";
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
                if (await _proxy.RegisterAsync(credentials))
                {
                    TempData["Success"] = "Register Successful.";
                    return RedirectToAction("Login", "User");
                }

                TempData["Error"] = "Username or email already taken.";
                return View();
            }
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong, please try again later.";
                return View();
            }
        }
    }
}
