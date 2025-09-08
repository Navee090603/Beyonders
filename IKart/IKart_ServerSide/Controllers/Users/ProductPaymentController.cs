using IKart_ServerSide.Models;
using IKart_Shared.DTOs.Payment;
using Razorpay.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;

// Alias the Payment classes to avoid ambiguity
using IKartPayment = IKart_ServerSide.Models.Payment;
using RazorpayPayment = Razorpay.Api.Payment;

namespace IKart_ServerSide.Controllers.Users
{
    [RoutePrefix("api/payments")]
    public class ProductPaymentController : ApiController
    {
        // Database connection
        private IKartEntities db = new IKartEntities();

        // Razorpay credentials (use your test keys here)
        private string razorpayKey = ConfigurationManager.AppSettings["RazorpayKey"];
        private string razorpaySecret = ConfigurationManager.AppSettings["RazorpaySecret"];

        // ✅ Get payment options like card balance, etc.
        [HttpGet]
        [Route("options/{userId}/{productId}")]
        public IHttpActionResult GetPaymentOptions(int userId, int productId)
        {
            var user = db.Users.Find(userId);
            var product = db.Products.Find(productId);

            if (user == null || product == null)
                return NotFound();

            var card = db.EMI_Card.FirstOrDefault(c => c.UserId == userId && c.IsActive == true);

            decimal cardBalance = (decimal)(card != null ? card.Balance : 0);

            return Ok(new
            {
                HasCard = card != null,
                CardType = card?.CardType,
                CardBalance = cardBalance,
                ProductCost = product.Cost
            });
        }

        // ✅ Create Razorpay order for payment
        [HttpPost]
        [Route("razorpay-order")]
        public IHttpActionResult CreateRazorpayOrder(OrderRequestDto request)
        {
            var product = db.Products.Find(request.ProductId);
            if (product == null)
                return NotFound();

            var client = new RazorpayClient(razorpayKey, razorpaySecret);

            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("amount", (int)(product.Cost * 100)); // amount in paise
            options.Add("currency", "INR");
            options.Add("payment_capture", 1);

            var order = client.Order.Create(options);

            return Ok(new
            {
                orderId = order["id"],
                amount = order["amount"],
                currency = order["currency"],
                productName = product.ProductName
            });
        }

        // ✅ Verify Razorpay payment after user completes it
        [HttpPost]
        [Route("razorpay-verify")]
        public IHttpActionResult VerifyRazorpayPayment(VerifyPaymentDto dto)
        {
            try
            {
                var client = new RazorpayClient(razorpayKey, razorpaySecret);

                Dictionary<string, string> attributes = new Dictionary<string, string>();
                attributes.Add("razorpay_order_id", dto.RazorpayOrderId);
                attributes.Add("razorpay_payment_id", dto.RazorpayPaymentId);
                attributes.Add("razorpay_signature", dto.RazorpaySignature);

                Utils.verifyPaymentSignature(attributes);

                // Save payment details
                var payment = new IKartPayment();
                payment.EmiCardId = null;
                payment.UserId = dto.UserId;
                payment.ProductId = dto.ProductId;
                payment.PaymentMethodId = db.Payment_Methods.FirstOrDefault(m => m.MethodName == "UPI")?.PaymentMethodId ?? 4;
                payment.ProcessingFee = 0;
                payment.TotalAmount = dto.Amount;
                payment.PaymentDate = DateTime.Now;
                payment.Status = "Paid";

                db.Payments.Add(payment);
                db.SaveChanges();

                return Ok(new { Message = "Payment verified successfully!", PaymentId = payment.PaymentId });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Payment verification failed: " + ex.Message));
            }
        }

        // ✅ Pay using user's card balance
        [HttpPost]
        [Route("pay-card")]
        public IHttpActionResult PayUsingCard(CardPaymentDto dto)
        {
            var user = db.Users.Find(dto.UserId);
            var product = db.Products.Find(dto.ProductId);
            var card = db.EMI_Card.FirstOrDefault(c => c.UserId == dto.UserId && c.IsActive == true);

            if (user == null || product == null || card == null)
                return NotFound();

            if (card.Balance < product.Cost)
                return BadRequest("Insufficient card balance.");

            // Deduct balance
            card.Balance -= product.Cost;

            // Save payment in database
            var payment = new IKartPayment();
            payment.EmiCardId = card.EmiCardId;
            payment.UserId = user.UserId;
            payment.ProductId = product.ProductId;
            payment.PaymentMethodId = db.Payment_Methods.FirstOrDefault(m => m.MethodName == "Card")?.PaymentMethodId ?? 5;
            payment.ProcessingFee = 0;
            payment.TotalAmount = product.Cost;
            payment.PaymentDate = DateTime.Now;
            payment.Status = "Paid";

            db.Payments.Add(payment);
            db.SaveChanges();

            return Ok(new { Message = "Payment completed using card balance!", PaymentId = payment.PaymentId });
        }
    }
}
