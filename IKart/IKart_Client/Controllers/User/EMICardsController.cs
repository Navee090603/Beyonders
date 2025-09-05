using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Web.Mvc;
using IKart_Shared.DTOs;
using IKart_Shared.DTOs.EMI_Card;
using Newtonsoft.Json;

namespace IKart_Client.Controllers.User
{
    public class EMICardsController : Controller
    {
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
                        var json = JsonConvert.SerializeObject(dto);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var res = client.PostAsync($"{apiBase}/request", content).Result;

                        if (res.IsSuccessStatusCode)
                        {
                            TempData["Message"] = "Card request submitted successfully. Awaiting admin approval.";
                            return RedirectToAction("Index");
                        }

                        ModelState.AddModelError("", res.Content.ReadAsStringAsync().Result);
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
