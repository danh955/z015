// <copyright file="TiingoSupportedTickerTypeConfiguration.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.Repository.EntityTypeConfiguration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// Tiingo supported stock ticker type configuration class.
    /// </summary>
    internal class TiingoSupportedTickerTypeConfiguration : IEntityTypeConfiguration<TiingoSupportedTickerEntity>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<TiingoSupportedTickerEntity> builder)
        {
            builder.ToTable("TiingoSupportedTicker");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Ticker).HasColumnName("Ticker").IsRequired();
            builder.Property(p => p.Exchange).HasColumnName("Exchange");
            builder.Property(p => p.AssetType).HasColumnName("AssetType");
            builder.Property(p => p.PriceCurrency).HasColumnName("PriceCurrency");
            builder.Property(p => p.StartDate).HasColumnName("StartDate").HasColumnType("date");
            builder.Property(p => p.EndDate).HasColumnName("EndDate").HasColumnType("date");
            builder.Property(p => p.DateAdded).HasColumnName("DateAdded").IsRequired().HasColumnType("datetime2(2)");
            builder.Property(p => p.DateUpdated).HasColumnName("DateUpdated").IsRequired().HasColumnType("datetime2(2)");
        }
    }
}