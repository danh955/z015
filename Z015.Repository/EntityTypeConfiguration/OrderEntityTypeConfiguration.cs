// <copyright file="OrderEntityTypeConfiguration.cs" company="None">
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
            builder.ToTable("OpenPosition");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.UserId).HasColumnName("UserId").IsRequired();
            builder.Property(s => s.Symbol).HasColumnName("Symbol").IsRequired();
            builder.Property(s => s.ActionType).HasColumnName("ActionType").IsRequired();
            builder.Property(s => s.Quantity).HasColumnName("Quantity").IsRequired();
            builder.Property(s => s.FillDate).HasColumnName("FillDate");

            builder.HasIndex(s => new { s.UserId, s.Symbol, s.ActionType }).IsUnique();

            // Many orders has one user.
            builder.HasOne<UserEntity>(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId);
        }
    }
}