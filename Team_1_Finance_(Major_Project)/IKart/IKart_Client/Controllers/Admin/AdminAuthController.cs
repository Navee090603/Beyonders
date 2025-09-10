using IKart_Shared.DTOs.Authentication;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace IKart_ClientSide.Controllers.Admin
{
    public class AdminAuthController : Controller
    {
        private readonly string apiBase = "https://localhost:44365/api/admin/auth/";

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(AdminLoginDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    var res = await client.PostAsJsonAsync(apiBase + "login", dto);

                    if (res.IsSuccessStatusCode)
                    {
                        var result = await res.Content.ReadAsAsync<dynamic>();

                        Session["AdminId"] = result.AdminId;
                        Session["AdminName"] = result.Username;

                        return RedirectToAction("Index", "Dashboard");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid username or password");
                        return View(dto);
                    }
                }
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "AdminAuth");
        }
    }
}
