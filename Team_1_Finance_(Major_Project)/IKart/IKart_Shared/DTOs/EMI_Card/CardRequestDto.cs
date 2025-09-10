using System;

using System.Collections.Generic;

using System.Linq;

using System.Text;

using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

using System.Web;

namespace IKart_Shared.DTOs.EMI_Card

{

    public class CardRequestDto

    {

        public int Card_Id { get; set; }

        [Required]

        public int UserId { get; set; }

        [Required]

        [RegularExpression("Gold|Diamond|Platinum", ErrorMessage = "Invalid card type")]

        public string CardType { get; set; }

        [Required, StringLength(50)]

        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Bank name must contain only alphabets")]

        public string BankName { get; set; }

        [Required]

        [RegularExpression(@"^\d{9,18}$", ErrorMessage = "Account number must be between 9 and 18 digits")]

        public string AccountNumber { get; set; }

        [Required]

        [RegularExpression(@"^[A-Z]{4}\d{7}$", ErrorMessage = "IFSC code must start with 4 capital letters followed by 7 digits")]

        public string IFSC_Code { get; set; }

        [Required]

        [RegularExpression(@"^\d{12}$", ErrorMessage = "Aadhaar number must be exactly 12 digits")]

        public string AadhaarNumber { get; set; }

        public bool IsVerified { get; set; }

    }

}
