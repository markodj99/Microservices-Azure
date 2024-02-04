using Common.Models.User;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace ClientStateless.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(Login login)
        {
            if (true)
            {
                HttpContext.Session.SetString("Email", login.Email);
                return RedirectToAction("Index", "Home");
            }
            else
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
        public IActionResult Register(Register register)
        {
            int a = 5;

            return View();
        }
    }
}
