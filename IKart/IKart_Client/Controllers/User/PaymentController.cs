using IKart_Shared.DTOs.EMI_Card;
using Razorpay.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace IKart_Client.Controllers.User
{
    public class PaymentController : Controller
    {
        // Show Payment Page
        public ActionResult Index()
        {
            if (Session["FeeAmount"] == null)
                return RedirectToAction("Index", "EMICards");

            var fee = (decimal)Session["FeeAmount"];

            // ✅ Load Razorpay keys from Web.config
            var key = ConfigurationManager.AppSettings["RazorpayKey"];
            var secret = ConfigurationManager.AppSettings["RazorpaySecret"];

            // ✅ Create Razorpay order
            RazorpayClient client = new RazorpayClient(key, secret);
            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("amount", (fee * 100)); // in paise
            options.Add("currency", "INR");
            options.Add("payment_capture", 1);

            Order order = client.Order.Create(options);

            // Pass values to view
            ViewBag.RazorpayKey = key;
            ViewBag.OrderId = order["id"].ToString();
            ViewBag.FeeAmount = fee;

            return View();
        }

        // Verify Razorpay Payment
        [HttpPost]
        public ActionResult VerifyPayment(string razorpay_payment_id, string razorpay_order_id, string razorpay_signature, int PaymentMethodId)
        {
            var secret = ConfigurationManager.AppSettings["RazorpaySecret"];

            // 🔐 Verify Razorpay signature
            string generated_signature;
            using (var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret)))
            {
                var rawData = razorpay_order_id + "|" + razorpay_payment_id;
                var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));
                generated_signature = BitConverter.ToString(hash).Replace("-", "").ToLower();
            }

            if (generated_signature != razorpay_signature)
            {
                TempData["Error"] = "Payment verification failed.";
                return RedirectToAction("Index", "EMICards");
            }

            // ✅ Get session data
            var dto = (CardRequestDto)Session["PendingCardRequest"];
            var documents = (List<HttpPostedFileBase>)Session["PendingDocuments"];
            var amount = (decimal)Session["FeeAmount"];

            // 🔐 Ignore SSL Certificate validation (for local dev only)
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            using (HttpClient client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri("https://localhost:44365/");

                // Save Card Request
                var response = client.PostAsJsonAsync("api/emicards/request", dto).Result;
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Failed to save card request.";
                    return RedirectToAction("Index", "EMICards");
                }

                // ✅ Deserialize response to strong type
                var result = response.Content.ReadAsAsync<CardResponse>().Result;
                if (result?.dto == null)
                {
                    TempData["Error"] = "Failed to retrieve Card ID from API response.";
                    return RedirectToAction("Index", "EMICards");
                }

                int cardId = result.dto.Card_Id;

                // Upload documents if any
                if (documents != null && documents.Count > 0)
                {
                    var form = new MultipartFormDataContent();
                    foreach (var file in documents)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            var streamContent = new StreamContent(file.InputStream);
                            streamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                            {
                                Name = file.FileName,
                                FileName = file.FileName
                            };
                            form.Add(streamContent, file.FileName, file.FileName);
                        }
                    }

                    client.PostAsync($"api/emicards/upload-documents/{cardId}", form).Wait();
                }

                // Save payment
                var paymentDto = new PaymentDto
                {
                    PaymentMethodId = PaymentMethodId,
                    Amount = amount
                };
                client.PostAsJsonAsync($"api/emicards/payfee/{cardId}", paymentDto).Wait();
            }

            TempData["Message"] = "Payment successful! Your card request has been submitted for admin approval.";
            return RedirectToAction("Index", "EMICards");
        }

        // Strongly typed class for API response
        public class CardResponse
        {
            public string message { get; set; }
            public CardRequestDto dto { get; set; }
        }

    }
}
