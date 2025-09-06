using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;
using IKart_Shared.DTOs.EMI_Card;
using Newtonsoft.Json;

namespace IKart_Client.Controllers.User
{
    public class PaymentController : Controller
    {
        private readonly string apiBase = "https://localhost:44365/api/emicards";

        // Payment page
        public ActionResult Index()
        {
            if (Session["PendingCardRequest"] == null || Session["FeeAmount"] == null)
            {
                TempData["Error"] = "No pending EMI card request found.";
                return RedirectToAction("Index", "EMICards");
            }

            ViewBag.FeeAmount = Session["FeeAmount"];
            return View();
        }

        [HttpPost]
        public ActionResult ConfirmPayment(int PaymentMethodId)
        {
            if (Session["PendingCardRequest"] == null || Session["UserId"] == null)
            {
                TempData["Error"] = "Session expired or invalid request.";
                return RedirectToAction("Index", "EMICards");
            }

            var dto = (CardRequestDto)Session["PendingCardRequest"];
            int userId = Convert.ToInt32(Session["UserId"]);
            decimal feeAmount = Convert.ToDecimal(Session["FeeAmount"]);
            var uploadedDocs = Session["PendingDocuments"] as List<HttpPostedFileBase>;

            try
            {
                using (var handler = new HttpClientHandler())
                {
                    // Ignore SSL errors for localhost testing
                    handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                    using (var client = new HttpClient(handler))
                    {
                        // 1️⃣ Send Card Request to API
                        dto.UserId = userId;
                        var jsonDto = JsonConvert.SerializeObject(dto);
                        var contentDto = new StringContent(jsonDto, Encoding.UTF8, "application/json");
                        var resRequest = client.PostAsync($"{apiBase}/request", contentDto).Result;

                        if (!resRequest.IsSuccessStatusCode)
                        {
                            ModelState.AddModelError("", "Failed to create card request: " + resRequest.Content.ReadAsStringAsync().Result);
                            ViewBag.FeeAmount = feeAmount;
                            return View("Index");
                        }

                        // Get Card_Id returned by API
                        var resultData = resRequest.Content.ReadAsStringAsync().Result;
                        var createdRequest = JsonConvert.DeserializeObject<dynamic>(resultData);
                        int cardId = createdRequest.dto.Card_Id;

                        // 2️⃣ Upload Documents
                        if (uploadedDocs != null && uploadedDocs.Count > 0)
                        {
                            using (var multipart = new MultipartFormDataContent())
                            {
                                foreach (var file in uploadedDocs)
                                {
                                    var fileContent = new ByteArrayContent(ReadFileBytes(file));
                                    fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                                    {
                                        Name = $"\"{file.FileName}\"",
                                        FileName = $"\"{file.FileName}\""
                                    };
                                    multipart.Add(fileContent, file.FileName, file.FileName);
                                }

                                var resDocs = client.PostAsync($"{apiBase}/upload-documents/{cardId}", multipart).Result;
                                if (!resDocs.IsSuccessStatusCode)
                                {
                                    ModelState.AddModelError("", "Document upload failed: " + resDocs.Content.ReadAsStringAsync().Result);
                                    ViewBag.FeeAmount = feeAmount;
                                    return View("Index");
                                }
                            }
                        }

                        // 3️⃣ Pay Joining Fee
                        var paymentDto = new PaymentDto
                        {
                            CardId = cardId,
                            UserId = userId,
                            PaymentMethodId = PaymentMethodId,
                            Amount = feeAmount
                        };

                        var jsonPayment = JsonConvert.SerializeObject(paymentDto);
                        var contentPayment = new StringContent(jsonPayment, Encoding.UTF8, "application/json");
                        var resPayment = client.PostAsync($"{apiBase}/payfee/{cardId}", contentPayment).Result;

                        if (!resPayment.IsSuccessStatusCode)
                        {
                            ModelState.AddModelError("", "Payment failed: " + resPayment.Content.ReadAsStringAsync().Result);
                            ViewBag.FeeAmount = feeAmount;
                            return View("Index");
                        }

                        // Clear session after successful payment
                        Session.Remove("PendingCardRequest");
                        Session.Remove("PendingDocuments");
                        Session.Remove("FeeAmount");

                        TempData["Message"] = "Payment successful. Await admin approval to activate your EMI card.";
                        return RedirectToAction("Index", "EMICards");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);
                ViewBag.FeeAmount = feeAmount;
                return View("Index");
            }
        }

        // Helper method to read HttpPostedFileBase into byte[]
        private byte[] ReadFileBytes(HttpPostedFileBase file)
        {
            using (var inputStream = file.InputStream)
            {
                byte[] bytes = new byte[file.ContentLength];
                inputStream.Read(bytes, 0, file.ContentLength);
                return bytes;
            }
        }
    }
}
