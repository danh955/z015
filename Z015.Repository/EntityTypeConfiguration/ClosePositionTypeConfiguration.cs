// <copyright file="ClosePositionTypeConfiguration.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.Repository.EntityTypeConfiguration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// Close position type configuration class.
    /// </summary>
    public class ClosePositionTypeConfiguration : IEntityTypeConfiguration<ClosePositionEntity>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<ClosePositionEntity> builder)
        {
            builder.ToTable("ClosePosition");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.PortfolioId).HasColumnName("PortfolioId").IsRequired();
            builder.Property(s => s.Symbol).HasColumnName("Symbol").IsRequired().HasMaxLength(16);
            builder.Property(s => s.BuyDate).HasColumnName("BuyDate").IsRequired().HasColumnType("date");
            builder.Property(s => s.Quantity).HasColumnName("Quantity").IsRequired().HasPrecision(19, 6);
            builder.Property(s => s.Purchase).HasColumnName("Purchase").IsRequired().HasPrecision(19, 6);
            builder.Property(s => s.Cost).HasColumnName("Cost").IsRequired().HasPrecision(19, 6);
            builder.Property(s => s.SellDate).HasColumnName("SellDate").IsRequired().HasColumnType("date");
            builder.Property(s => s.Proceeds).HasColumnName("Proceeds").IsRequired().HasPrecision(19, 6);

            builder.HasIndex(s => new { s.PortfolioId, s.Symbol, s.BuyDate, s.SellDate }).IsUnique();

            // One Portfolio has many close positions.
            builder.HasOne<PortfolioEntity>(c => c.Portfolio)
                .WithMany(p => p.ClosePositions)
                .HasForeignKey(c => c.PortfolioId);
        }
    }
}