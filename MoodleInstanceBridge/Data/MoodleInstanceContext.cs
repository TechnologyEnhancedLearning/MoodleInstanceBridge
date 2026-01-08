using Microsoft.EntityFrameworkCore;
using MoodleInstanceBridge.Models.Configuration;

namespace MoodleInstanceBridge.Data
{
    /// <summary>
    /// Database context for Moodle instance configuration
    /// </summary>
    public class MoodleInstanceContext : DbContext
    {
        public MoodleInstanceContext(DbContextOptions<MoodleInstanceContext> options)
            : base(options)
        {
        }

        public DbSet<InstanceConfig> MoodleInstanceConfigs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<InstanceConfig>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.ShortName)
                    .IsUnique()
                    .HasDatabaseName("IX_InstanceConfig_ShortName");

                entity.Property(e => e.ShortName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.BaseUrl)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.TokenSecretName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.EnabledEndpoints)
                    .HasMaxLength(1000);

                entity.Property(e => e.Weighting)
                    .HasDefaultValue(100);

                entity.Property(e => e.IsEnabled)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
}
