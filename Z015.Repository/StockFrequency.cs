// <copyright file="StockFrequency.cs" company="None">
// Free and open source code.
// </copyright>
namespace Z015.Repository
{
    /// <summary>
    /// Frequency of the stock data.
    /// </summary>
    public enum StockFrequency : byte
    {
        /// <summary>
        /// Daily.
        /// </summary>
        Daily = 1,

        /// <summary>
        /// Weekly.  Monday though Friday.  Date will be set on the Monday.
        /// </summary>
        Weekly = 2,

        /// <summary>
        /// Monthly.  First day of each month.
        /// </summary>
        Monthly = 3,

        /// <summary>
        /// Quarterly.  Starts on 1/1/YYYY, 4/1/YYYY, 7/1/YYYY and 10/1/YYYY.
        /// </summary>
        Quarterly = 4,

        /// <summary>
        /// Half Yearly.  Starts on 1/1/YYYY and 7/1/YYYY.
        /// </summary>
        HalfYearly = 5,

        /// <summary>
        /// Yearly.  From 1/1/YYYY to 12/31/YYYY.  The date will be set on January 1st.
        /// </summary>
        Yearly = 6,
    }
}