using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Models
{
    public class IDType
    {
        public long IdTypeId { get; set; }
        public string IdTypeName { get; set; }
        public string IdTypeDescription { get; set; }
        public DateTime DateCreated { get; set; }
        public string CreatedBy { get; set; }
        public DateTime DateUpdated { get; set; }
        public string UpdatedBy { get; set; }
    }
}