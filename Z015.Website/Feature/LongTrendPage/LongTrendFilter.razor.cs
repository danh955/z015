// <copyright file="LongTrendFilter.razor.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.Website.Feature.LongTrendPage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;
    using Z015.AppFeature.LongTrendPage;

    /// <summary>
    /// Long trend filter partial class.
    /// </summary>
    public partial class LongTrendFilter : ComponentBase
    {
        private readonly IEnumerable<int> endYears;
        private readonly LongTrendListOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="LongTrendFilter"/> class.
        /// </summary>
        public LongTrendFilter()
        {
            this.options = new();
            this.endYears = Enumerable.Range(this.options.EndYear - 19, 20).Reverse();
        }

        /// <summary>
        /// Gets or sets the options changed event.
        /// </summary>
        [Parameter]
        public EventCallback<LongTrendListOptions> OnOptionsChanged { get; set; }

        /// <inheritdoc/>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                await this.OnOptionsChanged.InvokeAsync(this.options);
            }
        }

        private async Task FormSubmitted(EditContext editContext)
        {
            await this.OnOptionsChanged.InvokeAsync(this.options);
        }
    }
}