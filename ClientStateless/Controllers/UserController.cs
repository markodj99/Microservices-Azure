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

        public IActionResult Login()
            => HttpContext.Session.GetString("Email") is not null ? RedirectToAction("Index", "Home") : View();

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
            HttpContext.Session.Remove("Basket");
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
            => HttpContext.Session.GetString("Email") is not null ? RedirectToAction("Index", "Home") : View();

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

                TempData["Error"] = "Email already taken. Login instead.";
                return View();
            }
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong, please try again later.";
                return View();
            }
        }

        public async Task<IActionResult> Edit()
        {
            if (HttpContext.Session.GetString("Email") is null) return RedirectToAction("Login", "User");

            var editProfile = await _proxy.GetUserDataAsync(HttpContext.Session.GetString("Email") ?? "Error");

            if (editProfile is null)
            {
                TempData["Error"] = "Something went wrong, please try again later.";
                return RedirectToAction("Index", "Home");
            }

            return View(editProfile);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditProfile credentials)
        {
            try
            {
                if (await _proxy.UpdateProfileAsync(credentials))
                {
                    TempData["Success"] = "Update Successful.";
                    return RedirectToAction("Index", "Home");
                }

                TempData["Error"] = "Form Wrongly Filled.";
                return RedirectToAction("Edit", "User");
            }
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong, please try again later.";
                return RedirectToAction("Edit", "User");
            }
        }
    }
}
