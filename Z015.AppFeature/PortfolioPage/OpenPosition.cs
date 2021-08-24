namespace Z015.AppFeature.PortfolioPage
{
    using System;

    /// <summary>
    /// Open position record.
    /// </summary>
    public record OpenPosition(int Id, string Symbol, DateTime BuyDate, decimal Quantity, decimal Purchase, decimal Cost);
}