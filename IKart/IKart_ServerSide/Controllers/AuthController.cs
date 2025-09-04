using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http;
using IKart_ServerSide.Models;
using IKart_Shared.DTOs.Authentication;

namespace IKart_ServerSide.Controllers.Users
{
    [RoutePrefix("api/user/auth")]
    public class AuthController : ApiController
    {
        IKartEntities db = new IKartEntities();

        // ✅ Register
        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register(UserRegisterDto dto)
        {
            // --- Basic Required Fields ---
            if (string.IsNullOrWhiteSpace(dto.FullName))
                return BadRequest("Full Name is required");
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Email is required");
            if (string.IsNullOrWhiteSpace(dto.PhoneNo))
                return BadRequest("Phone number is required");
            if (string.IsNullOrWhiteSpace(dto.Username))
                return BadRequest("Username is required");
            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Password is required");
            if (string.IsNullOrWhiteSpace(dto.ConfirmPassword))
                return BadRequest("Confirm Password is required");

            // --- Password Match ---
            if (dto.Password != dto.ConfirmPassword)
                return BadRequest("Password and Confirm Password do not match");

            // --- Email Format Validation ---
            if (!Regex.IsMatch(dto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return BadRequest("Invalid Email format");

            // --- Phone Number Validation ---
            if (!Regex.IsMatch(dto.PhoneNo, @"^[0-9]{10,15}$"))
                return BadRequest("Phone number must be 10–15 digits");

            // --- Password Strength Validation ---
            if (dto.Password.Length < 6)
                return BadRequest("Password must be at least 6 characters long");

            // --- Unique Validations ---
            if (db.Users.Any(u => u.Email == dto.Email))
                return BadRequest("Email already exists");
            if (db.Users.Any(u => u.PhoneNo == dto.PhoneNo))
                return BadRequest("Phone number already exists");
            if (db.Users.Any(u => u.Username == dto.Username))
                return BadRequest("Username already exists");

            // --- Create New User ---
            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = dto.Email.Trim(),
                PhoneNo = dto.PhoneNo.Trim(),
                Username = dto.Username.Trim(),
                PasswordHash = dto.Password, // ⚠️ should be hashed in production
                Status = "Active",
                CreatedDate = DateTime.Now
            };

            db.Users.Add(user);
            db.SaveChanges();

            return Ok(new
            {
                message = "Registration successful",
                user.UserId,
                user.FullName,
                user.Username
            });
        }

        // ✅ Login
        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(UserLoginDto dto)
        {
            // --- Basic Validation ---
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Username and Password are required");

            var user = db.Users
                .FirstOrDefault(u => u.Username == dto.Username && u.PasswordHash == dto.Password);

            if (user == null)
                return Unauthorized();

            return Ok(new
            {
                message = "Login successful",
                user.UserId,
                user.FullName,
                user.Username
            });
        }
    }
}
