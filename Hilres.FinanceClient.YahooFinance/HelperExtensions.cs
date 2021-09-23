// <copyright file="HelperExtensions.cs" company="None">
// Free and open source code.
// </copyright>

namespace Hilres.FinanceClient.YahooFinance
{
    using System;
    using System.Linq;
    using CsvHelper;

    /// <summary>
    /// For parsing data.
    /// </summary>
    internal static class HelperExtensions
    {
        /// <summary>
        /// Parse a string into a double.  Value of 0 if invalid data.
        /// </summary>
        /// <param name="csv">CsvReader.</param>
        /// <param name="index">Column index.</param>
        /// <returns>double value.</returns>
        internal static double? GetDouble(this CsvReader csv, int index)
        {
            return double.TryParse(csv[index], out double value) ? value : null;
        }

        /// <summary>
        /// Parse a string into a long.  Value of 0 if invalid data.
        /// </summary>
        /// <param name="csv">CsvReader.</param>
        /// <param name="index">Column index.</param>
        /// <returns>double value.</returns>
        internal static long? GetLong(this CsvReader csv, int index)
        {
            return long.TryParse(csv[index], out long value) ? value : null;
        }

        /// <summary>
        /// Convert date time to Unix time stamp.
        /// </summary>
        /// <param name="date">Date to convert.</param>
        /// <returns>Unix time stamp in string form.</returns>
        internal static string ToUnixTimestamp(this DateTime date) =>
            DateTime.SpecifyKind(date.FromEstToUtc(), DateTimeKind.Utc)
                .Subtract(Constant.EpochDate)
                .TotalSeconds
                .ToString();

        private static DateTime FromEstToUtc(this DateTime date) =>
            DateTime.SpecifyKind(date, DateTimeKind.Unspecified)
                .ToUtcFrom(Constant.EasternTimeZone);

        private static DateTime ToUtcFrom(this DateTime date, TimeZoneInfo timeZone) =>
            TimeZoneInfo.ConvertTimeToUtc(date, timeZone);
    }
}