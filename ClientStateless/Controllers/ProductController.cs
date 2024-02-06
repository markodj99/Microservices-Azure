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
            : View(await _proxy.GetAllProductsByCategoryAsync("Car"));

        public async Task<IActionResult> CarParts()
            => HttpContext.Session.GetString("Email") is null ? RedirectToAction("Login", "User")
            : View(await _proxy.GetAllProductsByCategoryAsync("CarPart"));

        [HttpPost]
        public IActionResult AddToBasket(string productName, int productPrice)
        {
            if (HttpContext.Session.GetString("Email") is null) return BadRequest();

            var basket = GetBasket();
            basket.AddItem(productName, productPrice);
            HttpContext.Session.SetString("Basket", JsonConvert.SerializeObject(basket));

            TempData["Success"] = $"Item {productName} Added Successfully.";
            return Ok();
        }

        [HttpPost]
        public IActionResult ReduceBasket(string productName)
        {
            if (HttpContext.Session.GetString("Email") is null) return BadRequest();

            var basket = GetBasket();
            basket.RemoveOne(productName);
            HttpContext.Session.SetString("Basket", JsonConvert.SerializeObject(basket));

            TempData["Success"] = $"Item {productName} Removed Successfully.";
            return Ok();
        }

        public IActionResult CheckOut()
        {
            if (HttpContext.Session.GetString("Email") is null) return RedirectToAction("Login", "User");
            return View(GetBasket());
        }

        [HttpPost]
        public async Task<IActionResult> MakePurchase(string paymentmethod)
        {
            if (HttpContext.Session.GetString("Email") is null) return BadRequest();

            var basket = GetBasket();
            if (basket.Items.Count == 0) return BadRequest();

            basket.PaymentMethod = paymentmethod;
            bool response = await _proxy.MakePurchaseAsync(basket);

            if (response)
            {
                HttpContext.Session.Remove("Basket");
                return Ok();
            }

            return BadRequest();
        }

        [HttpPost]
        public IActionResult ClearBasket()
        {
            if (HttpContext.Session.GetString("Email") is null) return BadRequest();
            HttpContext.Session.Remove("Basket");
            TempData["Success"] = "Basket Cleared Successfully.`";
            return Ok();
        }

        private Basket GetBasket()
        {
            string email = HttpContext.Session.GetString("Email") ?? "Error";
            string? jsonList = HttpContext.Session.GetString("Basket");
            Basket basket;

            if (jsonList is null) basket = new Basket(email);
            else basket = JsonConvert.DeserializeObject<Basket>(jsonList);

            return basket;
        }
    }
}
