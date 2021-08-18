// <copyright file="PortfolioEntityTypeConfiguration.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.Repository.EntityTypeConfiguration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// Portfolio entity type configuration class.
    /// </summary>
    public class PortfolioEntityTypeConfiguration : IEntityTypeConfiguration<PortfolioEntity>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<PortfolioEntity> builder)
        {
            builder.ToTable("Portfolio");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.UserId).HasColumnName("UserId").IsRequired();
            builder.Property(p => p.Name).HasColumnName("Name").IsRequired();

            builder.HasIndex(p => new { p.UserId, p.Name }).IsUnique();

            // One user has many Portfolios.
            builder.HasOne<UserEntity>(p => p.User)
                .WithMany(u => u.Portfolios)
                .HasForeignKey(p => p.UserId);
        }
    }
}