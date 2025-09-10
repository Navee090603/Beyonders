using IKart_ServerSide.Models;
using IKart_Shared.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace IKart_ServerSide.Controllers.Admin
{
    [RoutePrefix("api/reports")]
    public class ReportsController : ApiController
    {
        private readonly IKartEntities db = new IKartEntities();

        [HttpGet, Route("")]
        public IHttpActionResult GetReportsSummary()
        {
            var dto = new ReportsDto();

            dto.TotalEmiCards = db.EMI_Card.Count();

            dto.EmiCardsByType = db.EMI_Card
                .GroupBy(c => c.CardType)
                .ToDictionary(g => string.IsNullOrEmpty(g.Key) ? "Unknown" : g.Key, g => g.Count());

            dto.TotalUsers = db.Users.Count();
            dto.Users = db.Users.Select(u => new ReportUserDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNo = u.PhoneNo,
                Username = u.Username,
                Status = u.Status != null && u.Status.ToLower() == "active",
                CreatedDate = u.CreatedDate ?? DateTime.MinValue
            }).ToList();

            dto.TotalOrders = db.Orders.Count();
            dto.OrdersByRegion = db.Orders
                .GroupBy(o => o.Region)
                .ToDictionary(g => string.IsNullOrEmpty(g.Key) ? "Unknown" : g.Key, g => g.Count());

            dto.OrdersByCategory = db.Products
                .Join(db.Stocks, p => p.Stock_Id, s => s.Stock_Id, (p, s) => new { s.CategoryName })
                .GroupBy(x => string.IsNullOrEmpty(x.CategoryName) ? "Unknown" : x.CategoryName)
                .ToDictionary(g => g.Key, g => g.Count());

            dto.OrdersByPayment = db.Payments
                .GroupBy(p => p.PaymentMethodId)
                .Join(db.Payment_Methods, g => g.Key, pm => pm.PaymentMethodId,
                    (g, pm) => new { pm.MethodName, Count = g.Count() })
                .GroupBy(x => string.IsNullOrEmpty(x.MethodName) ? "Unknown" : x.MethodName)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Count));

            dto.Orders = db.Orders.Select(o => new ReportOrderDto
            {
                OrderId = o.Order_Id,
                UserFullName = o.User != null ? o.User.FullName : null,
                ProductName = o.Product != null ? o.Product.ProductName : null,
                Region = o.Region,
                OrderStatus = o.OrderStatus,
                OrderDate = o.OrderDate ?? DateTime.MinValue,
                DeliveryDate = o.DeliveryDate,
                TotalAmount = o.Payment != null ? o.Payment.TotalAmount : 0,
                PaymentMethod = o.Payment != null && o.Payment.Payment_Methods != null ? o.Payment.Payment_Methods.MethodName : null,
                CategoryName = o.Product != null && o.Product.Stock != null ? o.Product.Stock.CategoryName : null
            }).ToList();

            dto.TotalRefunds = db.Refunds.Count();
            dto.Refunds = db.Refunds.Select(r => new ReportRefundDto
            {
                RefundId = r.RefundId,
                PaymentId = r.PaymentId ?? 0,
                Amount = r.Amount ?? 0,
                Reason = r.Reason,
                Status = r.Status,
                RefundDate = r.RefundDate ?? DateTime.MinValue
            }).ToList();


            dto.TotalReturns = db.Returns.Count();
            dto.Returns = db.Returns.Select(r => new ReportReturnDto
            {
                ReturnId = r.ReturnId,
                OrderId = r.OrderId ?? 0,
                UserId = r.UserId ?? 0,
                ProductId = r.ProductId ?? 0,
                Reason = r.Reason,
                Status = r.Status,
                ReturnDate = r.ReturnDate ?? DateTime.MinValue
            }).ToList();

            dto.TotalCancellations = db.Order_Cancellations.Count();
            dto.Cancellations = db.Order_Cancellations.Select(c => new ReportCancellationDto
            {
                CancellationId = c.CancellationId,
                OrderId = c.OrderId ?? 0,
                UserId = c.UserId ?? 0,
                Reason = c.Reason,
                CancelDate = c.CancelDate ?? DateTime.MinValue,
                Refunded = c.Refunded ?? false
            }).ToList();

            return Ok(dto);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}

