// ⭐ FULL UPDATED AuthController (MVC)

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
            if (!ModelState.IsValid)
                return View(dto);

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    var res = await client.PostAsJsonAsync(apiBase + "register", dto);

                    if (res.IsSuccessStatusCode)
                    {
                        var result = await res.Content.ReadAsAsync<dynamic>();
                        TempData["UserId"] = result.UserId;
                        return RedirectToAction("VerifyOtp");
                    }

                    ModelState.AddModelError("", await res.Content.ReadAsStringAsync());
                    return View(dto);
                }
            }
        }

        // OTP Verification
        public ActionResult VerifyOtp()
        {
            if (TempData["UserId"] == null)
                return RedirectToAction("Register");

            ViewBag.UserId = TempData["UserId"];
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> VerifyOtp(int userId, string otp)
        {
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    var dto = new VerifyOtpDto { UserId = userId, Otp = otp };
                    var res = await client.PostAsJsonAsync(apiBase + "verify-otp", dto);

                    if (res.IsSuccessStatusCode)
                        return RedirectToAction("Login");

                    ModelState.AddModelError("", await res.Content.ReadAsStringAsync());
                    ViewBag.UserId = userId;
                    return View();
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
                        return RedirectToAction("Index", "Product");
                    }
                    else
                    {
                        var errorJson = await res.Content.ReadAsAsync<dynamic>();
                        string message = errorJson.message.ToString();

                        if (message.Contains("verify your account"))
                        {
                            ViewBag.ShowResendOtp = true; // ⭐ Flag for view
                            ViewBag.UserId = (int)errorJson.UserId; // ⭐ Pass UserId
                        }

                        ModelState.AddModelError("", message);
                        return View(dto);
                    }
                }
            }
        }

        // ✅ Resend OTP (MVC)
        [HttpPost]
        public async Task<ActionResult> ResendOtp(int userId)
        {
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    var res = await client.PostAsJsonAsync(apiBase + "resend-otp", userId);
                    if (res.IsSuccessStatusCode)
                    {
                        TempData["UserId"] = userId;
                        return RedirectToAction("VerifyOtp");
                    }

                    ModelState.AddModelError("", await res.Content.ReadAsStringAsync());
                    return RedirectToAction("Login");
                }
            }
        }
    }
}
