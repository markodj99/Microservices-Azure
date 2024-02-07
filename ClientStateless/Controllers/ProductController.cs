using Common.Interfaces;
using Common.Models.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json.Nodes;

namespace ClientStateless.Controllers
{
    [IgnoreAntiforgeryToken]
    public class ProductController : Controller
    {
        public string PayPalClientId { get; set; } = "";
        private string PayPalSecret { get; set; } = "";
        public string PayPalUrl { get; set; } = "";

        public ProductController(IConfiguration configuration)
        {
            PayPalClientId = configuration["PayPalSettings:ClientId"];
            PayPalSecret = configuration["PayPalSettings:Secret"];
            PayPalUrl = configuration["PayPalSettings:Url"];
        }

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

        public async Task<JsonResult> CreateOrder()
        {
            var basket = GetBasket();
            int totalAmount = 0;
            foreach (var item in basket.Items) totalAmount += (item.Quantity * item.Price);
            if (totalAmount == 0) return new JsonResult("");

            var createOrderRequest = new JsonObject
            {
                { "intent", "CAPTURE" }
            };

            var purchaseUnits = new JsonArray
            {
                new JsonObject
                {
                    { "amount", new JsonObject
                        {
                            { "currency_code", "USD" },
                            { "value", totalAmount }
                        }
                    }
                }
            };

            createOrderRequest.Add("purchase_units", purchaseUnits);

            string accessToken = await GetPayPalAccessToken();

            string url = PayPalUrl + "/v2/checkout/orders";

            string orderID = "";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Content = new StringContent(createOrderRequest.ToString(), null, "application/json");

                var response = await client.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    var read = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(read);
                    if (jsonResponse is not null)
                    {
                        orderID = jsonResponse["id"]?.ToString() ?? "";
                    }
                }
            }

            return new JsonResult(new { id = orderID } );
        }

        public async Task<JsonResult> CompleteOrder([FromBody] JsonObject data)
        {
            if (data is null || data["orderID"] is null) return new JsonResult("");

            var basket = GetBasket();
            if (!await _proxy.CanPurchaseAsync(basket.Items)) return new JsonResult("");

            var orderID = data["orderID"]!.ToString();
            
            string accessToken = await GetPayPalAccessToken();

            string url = PayPalUrl + $"/v2/checkout/orders/{orderID}/capture";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Content = new StringContent("", null, "application/json");

                var response = await client.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    var read = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(read);
                    if (jsonResponse is not null)
                    {
                        orderID = jsonResponse["id"]?.ToString() ?? "";
                        string payPalOrderStatus = jsonResponse["status"]?.ToString() ?? "";

                        if (payPalOrderStatus.Equals("COMPLETED"))
                        {
                            basket.PaymentMethod = "PayPal";
                            await _proxy.MakePurchaseAsync(basket);
                            HttpContext.Session.Remove("Basket");
                            return new JsonResult("success");
                        }
                    }
                }
            }

            return new JsonResult("");
        }

        private async Task<string> GetPayPalAccessToken()
        {
            string accessToken = "";

            string url = PayPalUrl + "/v1/oauth2/token";

            using(var client = new HttpClient())
            {
                string credentials64 =
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(PayPalClientId + ":" + PayPalSecret));
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + credentials64);

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Content = new StringContent("grant_type=client_credentials", null, 
                    "application/x-www-form-urlencoded");

                var response = await client.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    var read = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(read);
                    if (jsonResponse is not null)
                    {
                        accessToken = jsonResponse["access_token"]?.ToString() ?? "";
                    }
                }
            }

            return accessToken;
        }

        [HttpPost]
        public async Task<IActionResult> MakePurchaseCash()
        {
            if (HttpContext.Session.GetString("Email") is null) return BadRequest();

            var basket = GetBasket();
            if (basket.Items.Count == 0) return BadRequest();

            basket.PaymentMethod = "Cash";
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
