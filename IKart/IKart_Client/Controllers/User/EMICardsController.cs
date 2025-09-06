using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using IKart_Shared.DTOs.EMI_Card;

namespace IKart_Client.Controllers.User
{
    public class EMICardsController : Controller
    {
        // Show existing EMI cards
        public ActionResult Index()
        {
            // existing code to fetch user's EMI cards from API...
            return View();
        }

        // Select Card Type
        public ActionResult AddCard()
        {
            return View();
        }

        // Submit Card Request (temporarily stored in session until payment)
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

            // Store request temporarily in session
            Session["PendingCardRequest"] = dto;

            // Save uploaded documents temporarily
            var files = HttpContext.Request.Files;
            var tempDocs = new List<HttpPostedFileBase>();
            foreach (string key in files)
            {
                var file = files[key];
                if (file != null && file.ContentLength > 0)
                    tempDocs.Add(file);
            }
            Session["PendingDocuments"] = tempDocs;

            // Determine joining fee
            decimal fee;
            switch (dto.CardType)
            {
                case "Gold": fee = 1000; break;
                case "Diamond": fee = 2000; break;
                default: fee = 3000; break;
            }
            Session["FeeAmount"] = fee;

            // Redirect to payment page
            return RedirectToAction("Index", "Payment");
        }
    }
}
