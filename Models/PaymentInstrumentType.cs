using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Models
{
    public class PaymentInstrumentType
    {
        public long PaymentInstrumentTypeId { get; set; }
        public string PaymentInstrumentTypeName { get; set; }
        public bool IsWallet { get; set; }
        public bool IsActive { get; set; }
    }
}