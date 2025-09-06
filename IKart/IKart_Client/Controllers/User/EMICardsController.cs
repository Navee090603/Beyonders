using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Web.Mvc;
using IKart_Shared.DTOs;
using IKart_Shared.DTOs.EMI_Card;
using Newtonsoft.Json;
using IKart_ServerSide.Models;
using System.Web;

namespace IKart_Client.Controllers.User
{
    public class EMICardsController : Controller
    {
        //IKartEntities db = new IKartEntities();
        private readonly string apiBase = "https://localhost:44365/api/emicards";

        // ✅ Show existing cards
        public ActionResult Index()
        {
            if (Session["UserId"] == null)
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            List<EmiCardDto> cards = new List<EmiCardDto>();

            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;
                    using (HttpClient client = new HttpClient(handler))
                    {
                        var res = client.GetAsync($"{apiBase}/user/{userId}").Result;
                        if (res.IsSuccessStatusCode)
                        {
                            var data = res.Content.ReadAsStringAsync().Result;
                            cards = JsonConvert.DeserializeObject<List<EmiCardDto>>(data);
                        }
                        else
                        {
                            ModelState.AddModelError("", "Unable to fetch EMI cards.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error fetching EMI cards: " + ex.Message);
            }

            return View(cards ?? new List<EmiCardDto>());
        }

        // ✅ Select card type (Gold/Diamond/Platinum)
        public ActionResult AddCard()
        {
            return View();
        }

        // ✅ Request new card (docs upload)
        [HttpPost]
        public ActionResult RequestCard(CardRequestDto dto)
        {
            if (Session["UserId"] == null)
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            dto.UserId = Convert.ToInt32(Session["UserId"]);

            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;
                    using (HttpClient client = new HttpClient(handler))
                    {
                        // Step 1: Submit card request
                        var json = JsonConvert.SerializeObject(dto);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var res = client.PostAsync($"{apiBase}/request", content).Result;

                        if (res.IsSuccessStatusCode)
                        {
                            var responseData = res.Content.ReadAsStringAsync().Result;
                            var result = JsonConvert.DeserializeObject<dynamic>(responseData);
                            int cardId = result.dto.Card_Id;

                            // Step 2: Upload documents
                            var files = HttpContext.Request.Files;
                            var formContent = new MultipartFormDataContent();

                            foreach (string key in files)
                            {
                                var file = files[key];
                                if (file != null && file.ContentLength > 0)
                                {
                                    var streamContent = new StreamContent(file.InputStream);
                                    streamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                                    {
                                        Name = $"\"{key}\"",
                                        FileName = $"\"{file.FileName}\""
                                    };
                                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                                    formContent.Add(streamContent, key, file.FileName);
                                }
                            }

                            var uploadRes = client.PostAsync($"{apiBase}/upload-documents/{cardId}", formContent).Result;
                            if (uploadRes.IsSuccessStatusCode)
                            {
                                TempData["Message"] = "Card request and documents submitted successfully.";
                                return RedirectToAction("Index");
                            }
                            else
                            {
                                ModelState.AddModelError("", "Card created but document upload failed.");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", res.Content.ReadAsStringAsync().Result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error submitting request: " + ex.Message);
            }

            return View("AddCard", dto);
        }


        // ✅ Pay joining fee after admin approval
        public ActionResult PayFee(int feeId)
        {
            if (Session["UserId"] == null)
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;
                    using (HttpClient client = new HttpClient(handler))
                    {
                        var res = client.PostAsync($"{apiBase}/payfee/{feeId}", null).Result;

                        if (res.IsSuccessStatusCode)
                        {
                            TempData["Message"] = "Joining fee paid. Card activated!";
                            return RedirectToAction("Index");
                        }

                        ModelState.AddModelError("", res.Content.ReadAsStringAsync().Result);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error processing payment: " + ex.Message);
            }

            return RedirectToAction("Index");
        }
    }
}