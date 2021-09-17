﻿// <copyright file="OrderEntityTypeConfiguration.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.Repository.EntityTypeConfiguration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// Order entity type configuration class.
    /// </summary>
    public class OrderEntityTypeConfiguration : IEntityTypeConfiguration<OrderEntity>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<OrderEntity> builder)
        {
            builder.ToTable("Order");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.PortfolioId).HasColumnName("PortfolioId").IsRequired();
            builder.Property(s => s.Symbol).HasColumnName("Symbol").IsRequired().HasMaxLength(16);
            builder.Property(s => s.ActionType).HasColumnName("ActionType").IsRequired();
            builder.Property(s => s.Quantity).HasColumnName("Quantity").IsRequired().HasPrecision(19, 6);
            builder.Property(s => s.FillDate).HasColumnName("FillDate").IsRequired().HasColumnType("date");

            builder.HasIndex(s => new { s.PortfolioId, s.Symbol, s.ActionType }).IsUnique();

            // One Portfolio has many orders.
            builder.HasOne<PortfolioEntity>(o => o.Portfolio)
                .WithMany(p => p.Orders)
                .HasForeignKey(o => o.PortfolioId);
        }
    }
}