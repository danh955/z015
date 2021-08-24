// <copyright file="OpenPositionTable.razor.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.Website.Feature.PortfolioPage
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Components;
    using Z015.AppFeature.PortfolioPage;

    /// <summary>
    /// Open position table partial class.
    /// </summary>
    public partial class OpenPositionTable : ComponentBase
    {
        private List<OpenPosition> openPositions;

        /// <summary>
        /// Gets or sets the open position list service.
        /// </summary>
        [Inject]
        protected OpenPositionListService Service { get; set; }

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            this.openPositions = this.Service.GetOpenPositions();
            base.OnInitialized();
        }
    }
}