// <copyright file="LongTrend.razor.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.Website.Pages
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    using Z015.AppFeature.LongTrendPage;
    using Z015.Website.Feature.LongTrendPage;

    /// <summary>
    /// Long trend page partial class.
    /// </summary>
    public partial class LongTrend : ComponentBase
    {
        private LongTrendTable trendTable;

        /// <summary>
        /// Update long trend table.
        /// </summary>
        /// <param name="options">LongTrendListOptions.</param>
        /// <returns>Task.</returns>
        private async Task UpdateTable(LongTrendListOptions options)
        {
            await this.trendTable.UpdateLongTrendTable(options);
        }
    }
}