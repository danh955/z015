// <copyright file="LongTrendListOptions.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.AppFeature.LongTrendPage
{
    using System;

    /// <summary>
    /// Long trend list options class.
    /// </summary>
    public class LongTrendListOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LongTrendListOptions"/> class.
        /// </summary>
        public LongTrendListOptions()
        {
            DateTimeOffset endDate = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, Constant.EasternTimeZone).AddMonths(-1);
            this.EndYear = endDate.Year;
            this.EndMonth = endDate.Month;
            this.FrequencyMonths = 12;
            this.ColumnCount = 10;
        }

        /// <summary>
        /// Gets or sets the last year to display.
        /// </summary>
        public int EndYear { get; set; }

        /// <summary>
        /// Gets or sets the last month to display.
        /// </summary>
        public int EndMonth { get; set; }

        /// <summary>
        /// Gets or sets the column display frequency.  The number of months for each sample.
        /// </summary>
        public int FrequencyMonths { get; set; }

        /// <summary>
        /// Gets or sets the number of columns to display.
        /// </summary>
        public int ColumnCount { get; set; }
    }
}