using System.Collections.Generic;

using System.Linq;

using System.Web.Http;

using IKart_ServerSide.Models;

using IKart_Shared.DTOs;
 
namespace IKart_ServerSide.Controllers

{

    [RoutePrefix("api/payment")]

    public class PaymentController : ApiController

    {

        IKartEntities db = new IKartEntities();

        // GET: api/payment/address/user/{userId}

        [HttpGet]

        [Route("address/user/{userId}")]

        public IHttpActionResult GetUserAddresses(int userId)

        {

            var addresses = db.Addresses

                .Where(a => a.UserId == userId)

                .Select(a => new AddressDto

                {

                    AddressId = a.AddressId,

                    UserId = (int)a.UserId,

                    Street = a.Street,

                    City = a.City,

                    State = a.State,

                    ZipCode = a.ZipCode,

                    Country = a.Country

                }).ToList();

            return Ok(addresses);

        }

    }

}

