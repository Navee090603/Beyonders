using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKart_Shared.DTOs.Authentication
{
    public class UserRegisterDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }   // plain in demo, hashed in real
    }
}
