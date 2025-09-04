using IKart_Shared.DTOs;
using IKart_Shared.DTOs.Authentication;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace IKart_ClientSide.Controllers.User
{
    public class AuthController : Controller
    {
        private readonly string apiBase = "https://localhost:44365/api/user/auth/";

        // Register Page
        public ActionResult Register() => View();

        [HttpPost]
        public async Task<ActionResult> Register(UserRegisterDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    var res = await client.PostAsJsonAsync(apiBase + "register", dto);
                    if (res.IsSuccessStatusCode)
                        return RedirectToAction("Login");

                    ModelState.AddModelError("", await res.Content.ReadAsStringAsync());
                    return View(dto);
                }
            }
        }

        // Login Page
        public ActionResult Login() => View();

        [HttpPost]
        public async Task<ActionResult> Login(UserLoginDto dto)
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
                        Session["UserId"] = result.UserId;
                        Session["FullName"] = result.FullName;
                        Session["Username"] = result.Username;

                        return RedirectToAction("Index", "Dashboard");
                    }

                    ModelState.AddModelError("", "Invalid username or password");
                    return View(dto); // 👈 must return view with error
                }
            }
        }

    }
}
