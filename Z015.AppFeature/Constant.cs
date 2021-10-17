// <copyright file="Constant.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.AppFeature
{
    using System;
    using System.Linq;

    /// <summary>
    /// Constant value class.
    /// </summary>
    internal class Constant
    {
        /// <summary>
        /// Eastern time zone identifier.
        /// </summary>
        internal static readonly TimeZoneInfo EasternTimeZone = TimeZoneInfo
            .GetSystemTimeZones()
            .Single(tz => tz.Id == "Eastern Standard Time" || tz.Id == "America/New_York");
    }
}