// <copyright file="ResultTable.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.AppFeature.LongTrendPage
{
    using System.Collections.Generic;

    /// <summary>
    /// Result table.
    /// Each row has a veritable number of columns.
    /// </summary>
    public record ResultTable(IEnumerable<string> Header, IEnumerable<ResultRow> Rows);
}