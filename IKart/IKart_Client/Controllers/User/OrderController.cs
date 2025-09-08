using IKart_Shared.DTOs;
using IKart_Shared.DTOs.Orders;
using IKart_Shared.DTOs.Payment;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace IKart_Client.Controllers.User
{
    public class OrderController : Controller
    {
        string baseUrl = "https://localhost:44365/api/orders";

        [HttpPost]
        public async Task<ActionResult> BuyNow(int productId)
        {
            int userId = Convert.ToInt32(Session["UserId"]);
            List<AddressDto> addresses = new List<AddressDto>();

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;
                using (HttpClient client = new HttpClient(handler))
                {
                    var response = await client.GetAsync($"https://localhost:44365/api/account/address/user/{userId}");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        addresses = JsonConvert.DeserializeObject<List<AddressDto>>(json);
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to load addresses.");
                        return View("Error");
                    }
                }
            }

            ViewBag.ProductId = productId;
            ViewBag.UserId = userId;

            return View("BuyNow", addresses); // Make sure BuyNow.cshtml exists
        }

        [HttpPost]
        public async Task<ActionResult> ShowUPISummary(int productId, int addressId, int userId, string method)
        {
            ProductDto product = null;

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;
                using (HttpClient client = new HttpClient(handler))
                {
                    var res = await client.GetAsync($"https://localhost:44365/api/product/{productId}");
                    if (res.IsSuccessStatusCode)
                    {
                        var json = await res.Content.ReadAsStringAsync();
                        product = JsonConvert.DeserializeObject<ProductDto>(json);
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to fetch product details.");
                        return View("Error");
                    }
                }
            }

            ViewBag.Product = product;
            ViewBag.AddressId = addressId;
            ViewBag.UserId = userId;
            ViewBag.Method = method;

            return View("ShowUPISummary");
        }

        public ActionResult ChoosePayment(int productId, int addressId)
        {
            ViewBag.ProductId = productId;
            ViewBag.AddressId = addressId;
            ViewBag.UserId = Convert.ToInt32(Session["UserId"]);
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> PlaceOrder(int productId, int addressId, string method)
        {
            int userId = Convert.ToInt32(Session["UserId"]);
            decimal productCost = 0;
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;
                using (HttpClient client = new HttpClient(handler))
                {
                    var res = await client.GetAsync($"https://localhost:44365/api/product/{productId}");
                    if (res.IsSuccessStatusCode)
                    {
                        var json = await res.Content.ReadAsStringAsync();
                        var product = JsonConvert.DeserializeObject<ProductDto>(json);
                        productCost = (decimal)product.Cost;
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to fetch product price.");
                        return View("ChoosePayment");
                    }
                }
            }

            decimal deliveryFee = 100;
            decimal totalCost = productCost + deliveryFee;

            // Place order
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;
                using (HttpClient client = new HttpClient(handler))
                {
                    var order = new COD_UPI_OrdersDto
                    {
                        ProductId = productId,
                        UserId = userId,
                        PaymentType = method.ToUpper(), // "COD", "UPI", or "RAZORPAY"
                        PaymentStatus = (method.ToUpper() == "COD") ? "Pending" : "Paid",
                        OrderDate = DateTime.Now,
                        DeliveryDate = DateTime.Now.AddDays(5)
                    };

                    var json = JsonConvert.SerializeObject(order);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var res = await client.PostAsync("https://localhost:44365/api/orders/place", content);

                    if (res.IsSuccessStatusCode)
                    {
                        ViewBag.Message = "Order Confirmed!";
                        return View("PlaceOrder");
                    }

                    ModelState.AddModelError("", await res.Content.ReadAsStringAsync());
                    return View("ChoosePayment");
                }
            }
        }

        [HttpPost]
        public async Task<ActionResult> ShowRazorpaySummary(int productId, int addressId, int userId, string method)
        {
            ProductDto product = null;
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;
                using (HttpClient client = new HttpClient(handler))
                {
                    var res = await client.GetAsync($"https://localhost:44365/api/product/{productId}");
                    if (res.IsSuccessStatusCode)
                    {
                        var json = await res.Content.ReadAsStringAsync();
                        product = JsonConvert.DeserializeObject<ProductDto>(json);
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to fetch product details.");
                        return View("Error");
                    }
                }
            }
            ViewBag.Product = product;
            ViewBag.AddressId = addressId;
            ViewBag.UserId = userId;
            ViewBag.Method = method;
            ViewBag.RazorpayKey = "rzp_test_REOSbBMiZHhMMR"; // Store securely!

            return View("ShowRazorpaySummary");
        }

        [HttpPost]
        public async Task<JsonResult> RazorpayPaymentSuccess()
        {
            // Read POST body as JSON
            string body = "";
            using (var reader = new System.IO.StreamReader(Request.InputStream))
            {
                body = await reader.ReadToEndAsync();
            }
            // Parse JSON
            dynamic data = JsonConvert.DeserializeObject(body);

            string razorpay_payment_id = data.razorpay_payment_id;
            int productId = data.productId;
            int addressId = data.addressId;
            int userId = data.userId;
            decimal amount = Convert.ToDecimal(data.amount);

            // 1. Store payment details in DB
            var razorpayDto = new RazorpayPaymentDto
            {
                PaymentId = razorpay_payment_id,
                ProductId = productId,
                UserId = userId,
                AddressId = addressId,
                Amount = amount,
                PaymentDate = DateTime.Now
            };

            bool paymentSaved = false;
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;
                using (HttpClient client = new HttpClient(handler))
                {
                    var json = JsonConvert.SerializeObject(razorpayDto);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var res = await client.PostAsync("https://localhost:44365/api/payments/razorpay", content);

                    paymentSaved = res.IsSuccessStatusCode;
                }
            }

            // 2. Place order only if payment was saved
            bool orderPlaced = false;
            if (paymentSaved)
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;
                    using (HttpClient client = new HttpClient(handler))
                    {
                        var order = new COD_UPI_OrdersDto
                        {
                            ProductId = productId,
                            UserId = userId,
                            PaymentType = "RAZORPAY",
                            PaymentStatus = "Paid",
                            OrderDate = DateTime.Now,
                            DeliveryDate = DateTime.Now.AddDays(5)
                        };

                        var json = JsonConvert.SerializeObject(order);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var res = await client.PostAsync("https://localhost:44365/api/orders/place", content);

                        orderPlaced = res.IsSuccessStatusCode;
                    }
                }
            }

            return Json(new { success = paymentSaved, orderPlaced = orderPlaced });
        }
    }
}