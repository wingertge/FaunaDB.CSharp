using System;

namespace FaunaDB.LINQ.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Converts a DateTime object to seconds since the unix epoch
        /// </summary>
        /// <param name="target">The DateTime object to return</param>
        /// <returns>The time since unix epoch in seconds</returns>
        public static long ToUnixTimeStamp(this DateTime target)
        {
            var date = new DateTime(1970, 1, 1, 0, 0, 0, target.Kind);
            var unixTimestamp = Convert.ToInt64((target - date).TotalSeconds);

            return unixTimestamp;
        }
    }
}