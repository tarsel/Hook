using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Models
{
    public class SubCounty
    {
        public long SubCountyId { get; set; }
        public string SubCountyName { get; set; }
        public string SubCountyDescription { get; set; }
        public DateTime DateCreated { get; set; }
        public string CreatedBy { get; set; }
        public DateTime DateUpdated { get; set; }
        public string UpdatedBy { get; set; }
    }
}