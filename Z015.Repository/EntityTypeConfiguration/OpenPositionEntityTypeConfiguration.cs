// <copyright file="OpenPositionEntityTypeConfiguration.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.Repository.EntityTypeConfiguration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// Open position entity type configuration class.
    /// </summary>
    public class OpenPositionEntityTypeConfiguration : IEntityTypeConfiguration<OpenPositionEntity>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<OpenPositionEntity> builder)
        {
            builder.ToTable("OpenPosition");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.UserId).HasColumnName("UserId").IsRequired();
            builder.Property(s => s.Symbol).HasColumnName("Symbol").IsRequired();
            builder.Property(s => s.BuyDate).HasColumnName("BuyDate").IsRequired();
            builder.Property(s => s.Quantity).HasColumnName("Quantity").IsRequired();
            builder.Property(s => s.Purchase).HasColumnName("Purchase").IsRequired();
            builder.Property(s => s.Cost).HasColumnName("Cost").IsRequired();

            builder.HasIndex(s => new { s.UserId, s.Symbol, s.BuyDate }).IsUnique();

            // Many open positions has one user.
            builder.HasOne<UserEntity>(o => o.User)
                .WithMany(u => u.OpenPositions)
                .HasForeignKey(o => o.UserId);
        }
    }
}