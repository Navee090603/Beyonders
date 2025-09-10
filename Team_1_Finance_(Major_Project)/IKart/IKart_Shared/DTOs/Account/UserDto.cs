using System;

using System.Collections.Generic;

using System.Linq;

using System.Text;

using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace IKart_Shared.DTOs

{

    public class UserDto

    {

        public int UserId { get; set; }

        [Required, StringLength(100)]

        public string FullName { get; set; }

        [Required]

        [EmailAddress(ErrorMessage = "Invalid email format")]

        [RegularExpression(@"^[^@\s]+@[^@\s]+\.(com)$", ErrorMessage = "Email must end with '.com'")]

        public string Email { get; set; }

        [Required]

        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits")]

        public string PhoneNo { get; set; }

        [Required, StringLength(50)]

        public string Username { get; set; }

        public string Password { get; set; }

    }

}
