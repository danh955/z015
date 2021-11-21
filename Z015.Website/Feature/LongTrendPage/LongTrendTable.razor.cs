// <copyright file="LongTrendTable.razor.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.Website.Feature.LongTrendPage
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    using Z015.AppFeature.LongTrendPage;

    /// <summary>
    /// Long trend table partial class.
    /// </summary>
    public partial class LongTrendTable : ComponentBase
    {
        private ResultTable resultTable;

        /// <summary>
        /// Gets or sets the long trend list service.
        /// </summary>
        [Inject]
        protected LongTrendListService Service { get; set; }

        /// <inheritdoc/>
        protected override async Task OnInitializedAsync()
        {
            if (this.resultTable == null)
            {
                this.resultTable = await this.Service.GetLongTrends(10);
            }

            await base.OnInitializedAsync();
        }

        private static string UpDownColor(double? percentage)
        {
            return percentage switch
            {
                null => string.Empty,
                _ when percentage > 0 => "up",
                _ when percentage < 0 => "down",
                _ => string.Empty,
            };
        }
    }
}