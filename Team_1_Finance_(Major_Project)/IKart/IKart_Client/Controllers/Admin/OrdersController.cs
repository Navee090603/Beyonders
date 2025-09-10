using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IKart_Shared.DTOs;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using IKart_Shared.DTOs.Admin;
using IKart_ClientSide.Filters;

namespace IKart_Client.Controllers
{
    [AdminAuthorize]
    public class OrdersController : Controller
    {
        private readonly string apiBase = "https://localhost:44365/api/admin/orders"; 

        public async Task<ActionResult> Index()
        {
            using (var handler = new HttpClientHandler())

            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                using (var client = new HttpClient(handler))
                {
                    var response = await client.GetStringAsync(apiBase);
                    var orders = JsonConvert.DeserializeObject<List<OrderDto>>(response);
                    return View(orders);
                }
            }
        }

        public async Task<ActionResult> Details(int id)
        {
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    var response = await client.GetAsync($"{apiBase}/{id}");

                    if (!response.IsSuccessStatusCode)
                    {
                        ViewBag.Error = $"Unable to fetch order details. Server returned: {response.StatusCode}";
                        return View("Error"); 
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    var order = JsonConvert.DeserializeObject<OrderDetailsDto>(json);
                    return View(order);
                }
            }
        }
    }
}