using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Models
{
    public class Organization
    {
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string ContactPersonName { get; set; }
        public string EmailAddress { get; set; }
        public string ContactPhoneNumber { get; set; }
        public string PhysicalLocation { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedByUsername { get; set; }
        public bool IsActive { get; set; }
    }
}