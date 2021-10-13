﻿// <copyright file="StockEntityTypeConfiguration.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.Repository.EntityTypeConfiguration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// Stock entity type configuration class.
    /// </summary>
    internal class StockEntityTypeConfiguration : IEntityTypeConfiguration<StockEntity>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<StockEntity> builder)
        {
            builder.ToTable("Stock");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Symbol).HasColumnName("Symbol").IsRequired();
            builder.Property(s => s.Name).HasColumnName("Name");
            builder.Property(s => s.Exchange).HasColumnName("Exchange");
            builder.Property(s => s.PriceUpdatedDate).HasColumnName("PriceUpdatedDate").HasColumnType("datetimeoffset(2)");
            builder.Property(s => s.IsSymbolNotFound).HasColumnName("IsSymbolNotFound");

            builder.HasIndex(s => new { s.Symbol });
            builder.HasIndex(s => new { s.Exchange, s.Symbol }).IsUnique();
            builder.HasIndex(s => new { s.PriceUpdatedDate });
        }
    }
}