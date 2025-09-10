using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IKart_Client.Controllers.User
{
    public class InfoController : Controller
    {
        public ActionResult ConditionsOfUse()
        {
            return View();
        }

        public ActionResult PrivacyNotice()
        {
            return View();
        }
    }
}