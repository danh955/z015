// <copyright file="Constant.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.Repository.UpdateBackground
{
    using System;
    using System.Linq;

    /// <summary>
    /// Constant value class.
    /// </summary>
    public static class Constant
    {
        /// <summary>
        /// Eastern time zone identifier.
        /// </summary>
        internal static readonly TimeZoneInfo EasternTimeZone = TimeZoneInfo
            .GetSystemTimeZones()
            .Single(tz => tz.Id == "Eastern Standard Time" || tz.Id == "America/New_York");

        /// <summary>
        /// The time the stock market closes is at 8:00pm EST.
        /// </summary>
        internal static readonly TimeSpan MarketClosedTime = new(12 + 8, 0, 0);
    }
}