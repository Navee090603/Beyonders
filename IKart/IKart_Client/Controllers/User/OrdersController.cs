using System;

using System.Collections.Generic;

using System.Net.Http;

using System.Net.Http.Formatting; // ✅ Required for PostAsJsonAsync

using System.Web.Mvc;

using IKart_Shared.DTOs;
using IKart_Shared.DTOs.Orders;

using Newtonsoft.Json;

namespace IKart_Client.Controllers

{

    public class OrderController : Controller

    {

        string baseUrl = "https://localhost:44365/api/account";

        [HttpPost]

        public ActionResult BuyNow(int productId)

        {

            TempData["ProductId"] = productId;

            int userId = Convert.ToInt32(Session["UserId"]); // Replace with actual logged-in user logic

            List<AddressDto> addresses = new List<AddressDto>();

            using (var handler = new HttpClientHandler())

            {

                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (HttpClient client = new HttpClient(handler))

                {

                    var res = client.GetAsync($"{baseUrl}/address/user/{userId}").Result;

                    if (res.IsSuccessStatusCode)

                    {

                        var data = res.Content.ReadAsStringAsync().Result;

                        addresses = JsonConvert.DeserializeObject<List<AddressDto>>(data);

                    }

                }

            }

            return View("SelectAddress", addresses);

        }

        [HttpPost]

        public ActionResult ProceedToPayment(int addressId)

        {

            TempData["AddressId"] = addressId;

            return View("PaymentOptions");

        }

        [HttpPost]

        public ActionResult ConfirmPayment(string paymentMethod)

        {

            int productId = (int)TempData["ProductId"];

            int addressId = (int)TempData["AddressId"];

            int userId = 1; // Replace with actual logged-in user logic

            int paymentId = 0;

            if (paymentMethod == "COD")

            {

                paymentId = 1;

            }

            else if (paymentMethod == "UPI")

            {

                paymentId = 2;

            }

            else if (paymentMethod == "EMI")

            {

                paymentId = 3;

            }

            if (paymentId == 0)

            {

                ViewBag.Message = "Invalid payment method selected.";

                return View("OrderConfirmation");

            }

            if (paymentMethod == "EMI")

            {

                TempData["ProductId"] = productId;

                TempData["AddressId"] = addressId;

                return RedirectToAction("EmiCheckout", "Emi");

            }

            var order = new OrderDto

            {

                ProductId = productId,

                UserId = userId,

                PaymentId = paymentId,

                OrderDate = DateTime.Now,

                DeliveryDate = DateTime.Now.AddDays(5)

            };

            using (var handler = new HttpClientHandler())

            {

                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (HttpClient client = new HttpClient(handler))

                {

                    var response = client.PostAsJsonAsync("https://localhost:44365/api/order", order).Result;

                    if (!response.IsSuccessStatusCode)

                    {

                        ViewBag.Message = "Failed to place order. Please try again.";

                        return View("OrderConfirmation");

                    }

                }

            }

            ViewBag.Message = $"Order placed successfully using {paymentMethod}!";

            return View("OrderConfirmation");

        }

    }

}

