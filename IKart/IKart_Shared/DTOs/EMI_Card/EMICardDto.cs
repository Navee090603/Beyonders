using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace IKart_Shared.DTOs.EMI_Card
{
    public class EmiCardDto
    {
        public int EmiCardId { get; set; }

        [Required]
        public int UserId { get; set; }

        public string CardType { get; set; }
        public string CardNumber { get; set; }
        public decimal TotalLimit { get; set; }
        public decimal Balance { get; set; }
        public bool IsActive { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpireDate { get; set; }
    }
}
