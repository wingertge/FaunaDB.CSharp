using System;

namespace FaunaDB.LINQ.Extensions
{
    public static class DateTimeExtensions
    {
        public static long ToUnixTimeStamp(this DateTime target)
        {
            var date = new DateTime(1970, 1, 1, 0, 0, 0, target.Kind);
            var unixTimestamp = Convert.ToInt64((target - date).TotalSeconds);

            return unixTimestamp;
        }
    }
}