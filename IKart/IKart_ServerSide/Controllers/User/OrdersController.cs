using System.Linq;

using System.Web.Http;

using IKart_ServerSide.Models;

using IKart_Shared.DTOs.Orders;

namespace IKart_ServerSide.Controllers

{

    [RoutePrefix("api/order")]

    public class OrderController : ApiController

    {

        IKartEntities db = new IKartEntities();

        [HttpPost]

        [Route("")]

        public IHttpActionResult PlaceOrder(OrderDto dto)

        {

            if (!db.Users.Any(u => u.UserId == dto.UserId))

                return BadRequest("Invalid UserId");

            if (!db.Products.Any(p => p.ProductId == dto.ProductId))

                return BadRequest("Invalid ProductId");

            if (!db.Payments.Any(p => p.PaymentId == dto.PaymentId))

                return BadRequest("Invalid PaymentId");

            var order = new Order

            {

                ProductId = dto.ProductId,

                UserId = dto.UserId,

                PaymentId = dto.PaymentId,

                OrderDate = dto.OrderDate,

                DeliveryDate = dto.DeliveryDate

            };

            db.Orders.Add(order);

            db.SaveChanges();

            dto.OrderId = order.Order_Id;

            return Ok(dto);

        }

    }

}
