using IKart_ServerSide.Models;
using IKart_Shared.DTOs.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace IKart_ServerSide.Controllers.Users
{
    [RoutePrefix("api/payments")]
    public class ProductPaymentController : ApiController
    {
        private readonly IKartEntities db = new IKartEntities();

        // Get all RazorPayments for a user
        [HttpGet]
        [Route("user/{userId}")]
        public IHttpActionResult GetPaymentsByUser(int userId)
        {
            var paymentEntities = db.RazorPayments
                .Where(p => p.UserId == userId)
                .ToList();

            var payments = paymentEntities.Select(p => new UserPaymentDto
            {
                PaymentId = p.PaymentId,
                ProductName = p.Product?.ProductName ?? "N/A",
                TotalAmount = p.Amount,
                Status = "Paid",
                PaymentDate = p.PaymentDate
            }).ToList();

            return Ok(payments);
        }

        // Get a single RazorPayment by int PaymentId
        [HttpGet]
        [Route("details/{paymentId}")]
        public IHttpActionResult GetPaymentDetails(int paymentId)
        {
            var payment = db.RazorPayments.Find(paymentId);
            if (payment == null) return NotFound();

            var dto = new UserPaymentDto
            {
                PaymentId = payment.PaymentId,
                ProductName = payment.Product?.ProductName ?? "N/A",
                TotalAmount = payment.Amount,
                Status = "Paid",
                PaymentDate = payment.PaymentDate
            };

            return Ok(dto);
        }

        // Store a new Razorpay payment
        [HttpPost]
        [Route("razorpay")]
        public IHttpActionResult StoreRazorpayPayment(RazorpayPaymentDto dto)
        {
            try
            {
                var payment = new RazorPayment
                {
                    ProductId = dto.ProductId,
                    UserId = dto.UserId,
                    AddressId = dto.AddressId,
                    RazorPayTransactionId = dto.RazorPayTransactionId, // store Razorpay payment id (string)
                    Amount = dto.Amount,
                    PaymentDate = dto.PaymentDate
                };
                db.RazorPayments.Add(payment);
                db.SaveChanges();

                // Return the auto-generated PaymentId to client if needed
                return Ok(new { PaymentId = payment.PaymentId });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // For completeness: UPI/COD payments are still handled in main Payments table
        // If you want to show UPI/COD payments, add similar endpoints for db.Payments

        // Installment payment example (if you use EMI logic)
        [HttpPost]
        [Route("pay-installment/{installmentId}")]
        public IHttpActionResult PayInstallment(int installmentId)
        {
            var installment = db.Installment_Payments.Find(installmentId);
            if (installment == null) return NotFound();

            installment.IsPaid = true;
            db.SaveChanges();

            return Ok();
        }
    }
}