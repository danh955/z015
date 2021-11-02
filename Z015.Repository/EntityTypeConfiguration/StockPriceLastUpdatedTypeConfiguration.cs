// <copyright file="StockPriceLastUpdatedTypeConfiguration.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.Repository.EntityTypeConfiguration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// Stock price last updated type configuration class.
    /// </summary>
    internal class StockPriceLastUpdatedTypeConfiguration : IEntityTypeConfiguration<StockPriceLastUpdatedEntity>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<StockPriceLastUpdatedEntity> builder)
        {
            builder.ToTable("StockPriceLastUpdated");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.StockId).HasColumnName("StockId").IsRequired();
            builder.Property(s => s.Frequency).HasColumnName("Frequency").IsRequired();
            builder.Property(s => s.PriceUpdatedDate).HasColumnName("PriceUpdatedDate");

            builder.HasIndex(s => new { s.StockId, s.Frequency }).IsUnique();
            builder.HasIndex(s => new { s.Frequency, s.PriceUpdatedDate });

            // Stock prices last updated has one stock.
            builder.HasOne<StockEntity>(p => p.Stock)
                .WithMany(s => s.StockPriceLastUpdates)
                .HasForeignKey(p => p.StockId);
        }
    }
}