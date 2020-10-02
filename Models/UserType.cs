using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Models
{
    public class UserType
    {
        public long UserTypeId { get; set; }
        public string UserTypeName { get; set; }
        public string UserTypeDescription { get; set; }
        public DateTime DateCreated { get; set; }
        public string CreatedBy { get; set; }
        public DateTime DateUpdated { get; set; }
        public string UpdatedBy { get; set; }
    }
}