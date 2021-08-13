// <copyright file="UserEntityTypeConfiguration.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.Repository.EntityTypeConfiguration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// User entity type configuration class.
    /// </summary>
    public class UserEntityTypeConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            builder.ToTable("User");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id).HasColumnName("Id").IsRequired();
            builder.Property(s => s.LoginName).HasColumnName("LoginName").IsRequired();

            builder.HasIndex(s => s.LoginName).IsUnique();
        }
    }
}