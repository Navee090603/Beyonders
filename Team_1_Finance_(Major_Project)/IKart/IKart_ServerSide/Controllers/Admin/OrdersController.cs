using IKart_ServerSide.Models;

using IKart_Shared.DTOs.Admin;

using System;

using System.Collections.Generic;

using System.Linq;

using System.Net;

using System.Web.Http;

using System.Data.Entity;

namespace IKart_ServerSide.Controllers.Admin

{

    [RoutePrefix("api/admin/orders")]

    public class OrdersController : ApiController

    {

        private readonly IKartEntities db = new IKartEntities();


        [HttpGet]

        [Route("")]

        public IHttpActionResult GetOrders()

        {

            var orders = db.Orders

                .Include(o => o.User)

                .Include(o => o.Payment)

                .Include(o => o.Product)

                .ToList() 

                .Select(o => new OrderDto

                {

                    Order_Id = o.Order_Id,

                    UserName = o.User?.FullName ?? "Unknown",

                    Status = o.Payment?.Status ?? "Pending",

                    TotalAmount = o.Payment?.TotalAmount ?? 0m,

                    OrderDate = o.OrderDate ?? DateTime.MinValue

                }).ToList();

            return Ok(orders);

        }



        [HttpGet]

        [Route("{id:int}")]

        public IHttpActionResult GetOrderDetails(int id)

        {

            try

            {

                var order = db.Orders

                    .Include(o => o.User)

                    .Include(o => o.Product)

                    .Include(o => o.Payment.Monthly_EMI_Calc.Select(m => m.Installment_Payments))

                    .Include(o => o.Payment.Payment_Methods)

                    .FirstOrDefault(o => o.Order_Id == id);

                if (order == null)

                    return NotFound();

                var emi = order.Payment?.Monthly_EMI_Calc?.FirstOrDefault();

                var installmentPayments = emi?.Installment_Payments?.ToList() ?? new List<Installment_Payments>();

                int paidInstallments = installmentPayments.Count(i => i.IsPaid == true);

                int tenureMonths = emi?.TenureMonths ?? 0;

                int monthsRemaining = tenureMonths - paidInstallments;

                decimal paidAmount = emi != null

                    ? (emi.TotalAmount ?? 0m) - (emi.RemainingAmount ?? 0m)

                    : (order.Payment?.TotalAmount ?? 0m);

                var details = new OrderDetailsDto

                {

                    Order_Id = order.Order_Id,

                    UserName = order.User?.FullName ?? "Unknown",

                    ProductName = order.Product?.ProductName ?? "N/A",

                    ProductCost = order.Product?.Cost ?? 0m,

                    PaymentType = order.Payment?.Payment_Methods?.MethodName ?? "Unknown",

                    PaidAmount = paidAmount,

                    RemainingAmount = emi?.RemainingAmount ?? 0m,

                    TenureMonths = tenureMonths,

                    MonthsRemaining = monthsRemaining,

                    OrderDate = order.OrderDate ?? DateTime.MinValue,

                    DeliveryDate = order.DeliveryDate,

                    Payments = installmentPayments.Select(i => new PaymentHistoryDto

                    {

                        PaymentDate = i.DueDate ?? DateTime.MinValue,

                        Amount = i.Amount ?? 0m,

                        Status = (i.IsPaid ?? false) ? "Paid" : "Pending"

                    }).ToList()

                };

                return Ok(details);

            }

            catch (Exception ex)

            {


                return InternalServerError(new Exception($"Error fetching order ID {id}: {ex.Message}", ex));

            }

        }

    }

}
