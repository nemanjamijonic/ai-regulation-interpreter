using Common.Models.DbModels;
using Common.Models.Constants;
using Microsoft.EntityFrameworkCore;

namespace Common.Context
{
    public class AIRegulationInterpreterContext : DbContext
    {
        public AIRegulationInterpreterContext(DbContextOptions options) : base(options) { }

        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentVersion> DocumentVersions { get; set; }
        // DocumentSections removed - will be in Vector Index only

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Document configuration
            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Version).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Type).IsRequired().HasConversion<string>();
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasMany(d => d.Versions)
                    .WithOne(v => v.Document)
                    .HasForeignKey(v => v.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.IsActive);
            });

            // DocumentVersion configuration
            modelBuilder.Entity<DocumentVersion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Version).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Changes).HasMaxLength(1000);
                entity.Property(e => e.FilePath).HasMaxLength(500);
                entity.Property(e => e.FileName).HasMaxLength(255);
                entity.Property(e => e.ValidFrom).IsRequired();
                
                // IndexStatus for vector indexing
                entity.Property(e => e.IndexStatus)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasDefaultValue(IndexStatus.Pending);
                
                entity.Property(e => e.IndexError).HasMaxLength(2000);

                entity.HasIndex(e => new { e.DocumentId, e.IsCurrent });
                entity.HasIndex(e => e.ValidFrom);
                entity.HasIndex(e => e.IndexStatus); // For background indexing jobs
            });
        }
    }
}
