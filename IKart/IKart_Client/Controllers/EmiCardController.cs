using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Web.Mvc;
using IKart_Shared.DTOs;
using Newtonsoft.Json;

namespace IKart_Client.Controllers
{
    public class EmiCardController : Controller
    {
        private readonly string apiBaseUrl = "https://localhost:44365/api/emicards";

        // GET: EmiCard
        public ActionResult Index()
        {
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                using (var client = new HttpClient(handler))
                {
                    var response = client.GetStringAsync(apiBaseUrl).Result; // synchronous call
                    var cards = JsonConvert.DeserializeObject<List<EmiCardDto>>(response);
                    return View(cards);
                }
            }
        }

        // GET: EmiCard/View/5
        public ActionResult View(int id)
        {
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                using (var client = new HttpClient(handler))
                {
                    var response = client.GetStringAsync($"{apiBaseUrl}/{id}").Result; // synchronous call
                    var card = JsonConvert.DeserializeObject<EmiCardDto>(response);
                    return View(card);
                }
            }
        }

        // POST: EmiCard/UpdateStatus/5
        [HttpPost]
        public ActionResult UpdateStatus(int id, string status)
        {
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                using (var client = new HttpClient(handler))
                {
                    var content = new StringContent($"\"{status}\"", Encoding.UTF8, "application/json");
                    var response = client.PutAsync($"{apiBaseUrl}/updatestatus/{id}", content).Result; // synchronous call

                    if (response.IsSuccessStatusCode)
                        TempData["Message"] = "Status updated successfully!";
                    else
                        TempData["Error"] = "Failed to update status.";
                }
            }
            return RedirectToAction("Index");
        }
    }
}
