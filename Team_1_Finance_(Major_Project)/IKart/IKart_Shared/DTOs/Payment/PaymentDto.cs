using System;

using System.Collections.Generic;

using System.Linq;

using System.Text;

using System.Threading.Tasks;

namespace IKart_Shared.DTOs.Payment

{
    public class PaymentDto

    {

        public int UserId { get; set; }

        public int ProductId { get; set; }

        public string MethodName { get; set; } 

        public int TenureMonths { get; set; }  

    }


    public class CardPaymentDto

    {

        public int UserId { get; set; }

        public int ProductId { get; set; }

        public int EmiCardId { get; set; }   

        public int TenureMonths { get; set; } 

    }



    public class VerifyPaymentDto

    {

        public string RazorpayOrderId { get; set; }

        public string RazorpayPaymentId { get; set; }

        public string RazorpaySignature { get; set; }

        public int UserId { get; set; }

        public int ProductId { get; set; }

        public decimal Amount { get; set; }
        public int InstallmentId { get; set; }
    }


    public class OrderRequestDto

    {

        public int ProductId { get; set; }

        public int UserId { get; set; }

    }

}