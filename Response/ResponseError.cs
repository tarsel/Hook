using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Response
{
    public class ResponseError
    {
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
    }
}