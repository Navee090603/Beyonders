using EMI_Card.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EMI_Card.Controllers
{
    public class CardController : Controller
    {
        public ActionResult Index()
        {
            var model = new EMICardViewModel
            {
                UserName = "Ravi Kumar",
                CardNumber = "1234 5678 9012 3456",
                Validity = "12/28",
                CreditLimit = 75000,
                CardType = "Platinum",
                ProfileImageUrl = Url.Content("~/images/user-profile.jpg") // Add this image to your project
            };

            return View(model);
        }
    }
}