using System;

namespace BeeInstant.NetSDK.Utils
{
    internal static class DateTimeHelpers
    {
        internal static long GetTimeStampInSeconds()
        {
            return Convert.ToInt64((DateTime.UtcNow - DateTime.MinValue).TotalSeconds);
        }
    }
}