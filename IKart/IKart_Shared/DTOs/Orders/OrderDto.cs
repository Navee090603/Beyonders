using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKart_Shared.DTOs.Orders
{
        public class OrderDto

        {

            public int OrderId { get; set; }

            public int ProductId { get; set; }

            public int UserId { get; set; }

            public int PaymentId { get; set; }

            public DateTime OrderDate { get; set; }

            public DateTime? DeliveryDate { get; set; }

        }
}
