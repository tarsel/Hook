using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Enums
{
    public enum TransactionState
    {
        Successful = 1,
        Failed = 2,
        Reversed = 3,
        Frozen = 4,
        PartiallyCommitted = 5,
    }
}