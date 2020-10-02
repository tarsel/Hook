using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Models
{
    public class CustomerType
    {
        public long CustomerTypeId { get; set; }
        public string CustomerTypeName { get; set; }
        public string CustomerTypeDescription { get; set; }
        public DateTime DateCreated { get; set; }
        public string CreatedBy { get; set; }
        public DateTime DateUpdated { get; set; }
        public string UpdatedBy { get; set; }
    }
}