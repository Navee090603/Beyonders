using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Mvc;
using IKart_Shared.DTOs;
using Newtonsoft.Json;

namespace IKart_Client.Controllers
{
    public class StocksController : Controller
    {
        string stocksApiUrl = "https://localhost:44365/api/stocks";

        // GET: Stocks/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Stocks/Create
        [HttpPost]
        public ActionResult Create(StocksDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                using (HttpClient client = new HttpClient(handler))
                {
                    var json = JsonConvert.SerializeObject(dto);
                    var data = new StringContent(json, Encoding.UTF8, "application/json");
                    var res = client.PostAsync(stocksApiUrl, data).Result;
                    if (res.IsSuccessStatusCode)
                        return RedirectToAction("Index", "Products"); // after adding stock, go back to products list
                }
            }

            return View(dto);
        }

        // =====================
        // Remove Stock
        // =====================
        // GET: Stocks/Remove
        public ActionResult Remove()
        {
            return View();
        }

        // POST: Stocks/Remove
        [HttpPost]
        public ActionResult Remove(StocksDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                using (HttpClient client = new HttpClient(handler))
                {
                    var json = JsonConvert.SerializeObject(dto);
                    var data = new StringContent(json, Encoding.UTF8, "application/json");
                    var res = client.PostAsync(stocksApiUrl + "/remove", data).Result; // endpoint: /api/stocks/remove
                    if (res.IsSuccessStatusCode)
                        return RedirectToAction("Index", "Products");
                }
            }

            return View(dto);
        }


    }
}
