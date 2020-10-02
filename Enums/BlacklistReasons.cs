using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Enums
{
    public enum BlacklistReasons
    {
        Active = 1,
        PinBlocked = 2,
        DeviceLost = 3,
        PaymentDispute = 4,
        FraudSuspicion = 5,
        RegistrationPending = 6,
        TACNotAccepted = 7,
        SuspectedAML = 8,
        PasswordBlocked = 9,
    }
}