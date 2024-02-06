using Common.Interfaces;
using Common.Models.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Newtonsoft.Json;

namespace ClientStateless.Controllers
{
    public class ProductController : Controller
    {
        private readonly IApiGateway _proxy
            = ServiceProxy.Create<IApiGateway>(new Uri("fabric:/Cloud-Project/ApiGatewayStateless"));

        public IActionResult All()
            => HttpContext.Session.GetString("Email") is null ? RedirectToAction("Login", "User") : View();

        public async Task<IActionResult> Cars()
            => HttpContext.Session.GetString("Email") is null ? RedirectToAction("Login", "User")
            : View(await _proxy.GetAllProductsByCategory("Car"));

        public async Task<IActionResult> CarParts()
            => HttpContext.Session.GetString("Email") is null ? RedirectToAction("Login", "User")
            : View(await _proxy.GetAllProductsByCategory("CarPart"));

        [HttpPost]
        public IActionResult AddToBasket(string productName)
        {
            if (HttpContext.Session.GetString("Email") is null) return BadRequest();

            string email = HttpContext.Session.GetString("Email") ?? "Error";
            string? jsonList = HttpContext.Session.GetString("Basket");
            Basket basket;

            if (jsonList is null) basket = new Basket(email);
            else basket = JsonConvert.DeserializeObject<Basket>(jsonList);

            basket.AddItem(productName);
            HttpContext.Session.SetString("Basket", JsonConvert.SerializeObject(basket));

            TempData["Success"] = $"Item {productName} Added Successfully.`";
            return Ok();
        }

        public IActionResult CheckOut()
        {
            if (HttpContext.Session.GetString("Email") is null) return RedirectToAction("Login", "User");

            return View();
        }
    }
}
