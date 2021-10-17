// <copyright file="ResultRow.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.AppFeature.LongTrendPage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Result row record.
    /// </summary>
    public record ResultRow(string Symbol, IEnumerable<double?> Percentages, double? LastClose, int Score);
}
