using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using IKart_Shared.DTOs.EMI_Card;

namespace IKart_Client.Controllers.User
{
    public class EMICardsController : Controller
    {
        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Auth");
            
            int userId = Convert.ToInt32(Session["UserId"]);
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                using (var client = new HttpClient(handler))
                {
                    var response = client.GetAsync($"https://localhost:44365/api/emicards/user/{userId}").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var data = response.Content.ReadAsAsync<List<IKart_Shared.DTOs.EMI_Card.EmiCardDto>>().Result;
                        return View(data);
                    }
                }
            }
            return View(new List<IKart_Shared.DTOs.EMI_Card.EmiCardDto>());
        }

        public ActionResult AddCard()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RequestCard(CardRequestDto dto)
        {
            if (Session["UserId"] == null)
            {
                TempData["Error"] = "Please login first.";
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
                return View("AddCard", dto);

            dto.UserId = Convert.ToInt32(Session["UserId"]);

            Session["PendingCardRequest"] = dto;

            var files = HttpContext.Request.Files;
            var tempDocs = new List<string>();
            var tempPath = Server.MapPath("~/TempDocs/");
            Directory.CreateDirectory(tempPath);

            foreach (string key in files)
            {
                var file = files[key];
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var fullPath = Path.Combine(tempPath, fileName);
                    file.SaveAs(fullPath);
                    tempDocs.Add(fileName);
                }
            }

            Session["PendingDocuments"] = tempDocs;

            decimal fee;
            switch (dto.CardType)
            {
                case "Gold": fee = 1000; break;
                case "Diamond": fee = 2000; break;
                default: fee = 3000; break;
            }
            Session["FeeAmount"] = fee;

            return RedirectToAction("Index", "Payment");
        }

        [HttpGet]

        public JsonResult ValidateBankName(string bankName)

        {

            if (string.IsNullOrWhiteSpace(bankName) || !System.Text.RegularExpressions.Regex.IsMatch(bankName, @"^[A-Za-z\s]+$"))

                return Json("Bank name must contain only alphabets", JsonRequestBehavior.AllowGet);

            return Json(true, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]

        public JsonResult ValidateAccountNumber(string accountNumber)

        {

            if (string.IsNullOrWhiteSpace(accountNumber) || !System.Text.RegularExpressions.Regex.IsMatch(accountNumber, @"^\d{9,18}$"))

                return Json("Account number must be 9–18 digits", JsonRequestBehavior.AllowGet);

            return Json(true, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]

        public JsonResult ValidateIFSC(string IFSC_Code)

        {

            IFSC_Code = IFSC_Code?.Trim().ToUpper();

            if (string.IsNullOrEmpty(IFSC_Code) || !System.Text.RegularExpressions.Regex.IsMatch(IFSC_Code, @"^[A-Z]{4}\d{7}$"))

                return Json("Invalid IFSC format", JsonRequestBehavior.AllowGet);

            return Json(true, JsonRequestBehavior.AllowGet);

        }



        [HttpGet]

        public JsonResult ValidateAadhaar(string aadhaarNumber)
        {

            if (string.IsNullOrWhiteSpace(aadhaarNumber) || !System.Text.RegularExpressions.Regex.IsMatch(aadhaarNumber, @"^\d{12}$"))

                return Json("Aadhaar must be exactly 12 digits", JsonRequestBehavior.AllowGet);

            return Json(true, JsonRequestBehavior.AllowGet);

        }

    }
}