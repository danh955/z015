// <copyright file="YahooPrice.cs" company="None">
// Free and open source code.
// </copyright>

namespace Hilres.FinanceClient.Abstraction
{
    using System;

    /// <summary>
    /// A stock price record from a data client.
    /// </summary>
    /// <param name="Date">Date of price.</param>
    /// <param name="Open">Opening price of the stock.</param>
    /// <param name="High">The high price.</param>
    /// <param name="Low">The low price.</param>
    /// <param name="Close">Closing price.</param>
    /// <param name="AdjClose">The adjusted closing price.</param>
    /// <param name="Volume">The volume traded.</param>

    public record YahooPrice(DateTime Date, double? Open, double? High, double? Low, double? Close, double? AdjClose, long? Volume);
}