using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKart_Shared.DTOs.Payment
{
    public class OrderRequestDto
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
    }

    public class VerifyPaymentDto
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public string RazorpayOrderId { get; set; }
        public string RazorpayPaymentId { get; set; }
        public string RazorpaySignature { get; set; }
        public decimal Amount { get; set; }
    }

    public class CardPaymentDto
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
    }
}
