using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IKart_Design.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        // GET: Admin/Home
        public ActionResult Index()
        {
            ViewBag.TotalSales = 125430.45m; // preview data — backend will replace
            ViewBag.PendingOrders = 14;
            ViewBag.TotalProducts = 1200;
            ViewBag.TotalCustomers = 5421;
            return View();
        }
    }
}