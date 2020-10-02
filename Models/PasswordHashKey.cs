using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Models
{
    public class PasswordHashKey
    {
        public long PasswordHashKeyId { get; set; }

        public string PINHash { get; set; }

        public string PinKeyIV { get; set; }

        public string SecurityCodeIV { get; set; }
    }
}