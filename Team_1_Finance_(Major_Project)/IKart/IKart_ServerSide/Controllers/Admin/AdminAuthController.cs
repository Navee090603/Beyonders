using IKart_ServerSide.Models;
using IKart_Shared.DTOs.Authentication;
using System.Linq;
using System.Web.Http;

namespace IKart_ServerSide.Controllers.Admin
{
    [RoutePrefix("api/admin/auth")]
    public class AdminAuthController : ApiController
    {
        IKartEntities db = new IKartEntities();

        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(AdminLoginDto dto)
        {
            var admin = db.Admins.FirstOrDefault(a => a.Username == dto.Username && a.PasswordHash == dto.Password);
            if (admin != null)
            {
                return Ok(new
                {
                    AdminId = admin.AdminId,
                    Username = admin.Username
                });
            }

            return Unauthorized();
        }

        [HttpGet]
        [Route("logout")]
        public IHttpActionResult Logout()
        {
            return Ok("Logged out successfully");
        }

    }
}
