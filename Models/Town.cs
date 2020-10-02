using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Models
{
    public class Town
    {
        public long TownId { get; set; }
        public string TownName { get; set; }
        public long SubCountyId { get; set; }
        public string TownDescription { get; set; }
        public DateTime DateCreated { get; set; }
        public string CreatedBy { get; set; }
        public DateTime DateUpdated { get; set; }
        public string UpdatedBy { get; set; }
    }
}