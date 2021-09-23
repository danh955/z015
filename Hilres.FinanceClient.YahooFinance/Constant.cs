// <copyright file="Constant.cs" company="None">
// Free and open source code.
// </copyright>

namespace Hilres.FinanceClient.YahooFinance
{
    using System;
    using System.Linq;

    /// <summary>
    /// Constant value class.
    /// </summary>
    internal static class Constant
    {
        /// <summary>
        /// Eastern time zone identifier.
        /// </summary>
        internal static readonly TimeZoneInfo EasternTimeZone = TimeZoneInfo
            .GetSystemTimeZones()
            .Single(tz => tz.Id == "Eastern Standard Time" || tz.Id == "America/New_York");

        /// <summary>
        /// The Unix epoch is the time 00:00:00 UTC on 1 January 1970.  (1970-01-01T00:00:00Z).
        /// </summary>
        internal static readonly DateTime EpochDate = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Epoch Unix time string.  Epoch is 1970-01-01T00:00:00Z.
        /// </summary>
        internal static readonly string EpochString = EpochDate.ToUnixTimestamp();
    }
}