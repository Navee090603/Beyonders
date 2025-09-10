using System;

namespace IKart_Shared.DTOs.Orders
{
    public class OrderDto
    {
        public int Order_Id { get; set; }      
        public int ProductId { get; set; }     
        public int UserId { get; set; }        
        public int? PaymentId { get; set; }  



        public DateTime OrderDate { get; set; }   
        public DateTime DeliveryDate { get; set; } 
    }

    public class COD_UPI_OrdersDto
    {
        public int ProductId { get; set; }    
        public int UserId { get; set; }  
        public string PaymentType { get; set; }
        public string PaymentStatus { get; set; } 
        public DateTime OrderDate { get; set; }   
        public DateTime DeliveryDate { get; set; } 
    }

    public class UserOrderDto
    {
        public int OrderId { get; set; }
        public string ProductName { get; set; }
        public string PaymentType { get; set; } 
        public string PaymentStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime DeliveryDate { get; set; }

    }
}
