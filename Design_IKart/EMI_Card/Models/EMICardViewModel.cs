using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EMI_Card.Models
{
    public class EMICardViewModel
    {
        public string UserName { get; set; }
        public string CardNumber { get; set; }
        public string Validity { get; set; }
        public decimal CreditLimit { get; set; }
        public string CardType { get; set; }
        public string ProfileImageUrl { get; set; }
    }

}