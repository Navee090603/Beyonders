using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKart_Shared.DTOs.Authentication
{
    public class LoginResponseDto
    {
        public string Message { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
    }
}
