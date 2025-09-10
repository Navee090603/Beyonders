using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using IKart_Shared.DTOs;
using Newtonsoft.Json;
using System.Linq;

namespace IKart_Client.Controllers
{
    public class AccountController : Controller
    {
        string baseUrl = "https://localhost:44365/api/account";

        public ActionResult User(int id)
        {
            UserDto user = null;
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;
                using (HttpClient client = new HttpClient(handler))
                {
                    var res = client.GetAsync($"{baseUrl}/user/{id}").Result;
                    if (res.IsSuccessStatusCode)
                    {
                        var data = res.Content.ReadAsStringAsync().Result;
                        user = JsonConvert.DeserializeObject<UserDto>(data);
                    }
                    else
                    {
                        ModelState.AddModelError("", res.Content.ReadAsStringAsync().Result);
                    }
                }
            }
            return View(user);
        }

        [HttpPost]
        public ActionResult User(int id, UserDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;
                using (HttpClient client = new HttpClient(handler))
                {
                    var json = JsonConvert.SerializeObject(dto);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var res = client.PutAsync($"{baseUrl}/user/{id}", content).Result;

                    if (res.IsSuccessStatusCode)
                        return RedirectToAction("User", new { id = dto.UserId });

                    ModelState.AddModelError("", res.Content.ReadAsStringAsync().Result);
                }
            }
            return View(dto);
        }


        public ActionResult Addresses()
        {
            var userId = Convert.ToInt32(Session["UserId"]); 
            var addresses = new List<AddressDto>();

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;
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
            return View(addresses);
        }


        public ActionResult EditAddress(int id)
        {
            AddressDto dto = null;
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;
                using (HttpClient client = new HttpClient(handler))
                {
                    var res = client.GetAsync($"{baseUrl}/address/{id}").Result;
                    if (res.IsSuccessStatusCode)
                    {
                        var data = res.Content.ReadAsStringAsync().Result;
                        dto = JsonConvert.DeserializeObject<AddressDto>(data);
                    }
                }
            }
            return View(dto);
        }


        [HttpPost]
        public ActionResult EditAddress(AddressDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;
                using (HttpClient client = new HttpClient(handler))
                {
                    var json = JsonConvert.SerializeObject(dto);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var res = client.PutAsync($"{baseUrl}/address/{dto.AddressId}", content).Result;

                    if (res.IsSuccessStatusCode)
                        return RedirectToAction("Addresses");

                    ModelState.AddModelError("", res.Content.ReadAsStringAsync().Result);
                }
            }
            return View(dto);
        }


        public ActionResult AddAddress()
        {
            var userId = Convert.ToInt32(Session["UserId"]);
            return View(new AddressDto { UserId = userId });
        }


        [HttpPost]
        public async Task<ActionResult> AddAddress(AddressDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            dto.UserId = Convert.ToInt32(Session["UserId"]); 

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    var res = await client.PostAsJsonAsync(baseUrl + "/address", dto); 

                    if (res.IsSuccessStatusCode)
                        return RedirectToAction("Addresses");

                    ModelState.AddModelError("", await res.Content.ReadAsStringAsync());
                }
            }

            return View(dto);
        }


        [HttpPost]
        public ActionResult DeleteAddress(int id)
        {
            var userId = Convert.ToInt32(Session["UserId"]);
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;
                using (HttpClient client = new HttpClient(handler))
                {
                    var res = client.DeleteAsync($"{baseUrl}/address/{id}").Result;
                    if (res.IsSuccessStatusCode)
                        return RedirectToAction("Addresses");
                }
            }
            return RedirectToAction("Addresses");
        }



        public ActionResult Profile()

        {

            if (Session["UserId"] == null)

                return RedirectToAction("Login", "Auth");

            int userId = Convert.ToInt32(Session["UserId"]);

            UserDto user = null;

            List<AddressDto> addresses = new List<AddressDto>();


            using (var handler = new HttpClientHandler())

            {

                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;

                using (HttpClient client = new HttpClient(handler))

                {

                    var res = client.GetAsync($"{baseUrl}/user/{userId}").Result;

                    if (res.IsSuccessStatusCode)

                    {

                        var data = res.Content.ReadAsStringAsync().Result;

                        user = JsonConvert.DeserializeObject<UserDto>(data);

                    }

                }

            }

            using (var handler = new HttpClientHandler())

            {

                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;

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

            ViewBag.Addresses = addresses;

            return View(user);

        }



        [HttpGet]
        public JsonResult ValidateZipCode(string ZipCode)

        {

            bool isValid = ZipCode.All(char.IsDigit) && ZipCode.Length == 6;

            return Json(isValid, JsonRequestBehavior.AllowGet);
        }


        }
    }
