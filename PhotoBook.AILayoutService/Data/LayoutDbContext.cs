using Microsoft.EntityFrameworkCore;

namespace PhotoBook.AILayoutService.Data;

public class LayoutDbContext : DbContext
{
    public LayoutDbContext(DbContextOptions<LayoutDbContext> options)
        : base(options) { }

    public DbSet<LayoutSuggestionCache> LayoutCache { get; set; }
    public DbSet<PhotoAnalysisCache> PhotoAnalysisCache { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LayoutSuggestionCache>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CacheKey).IsUnique();
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.ExpiresAt);

            entity.Property(e => e.SuggestionsJson)
                .HasColumnType("jsonb");
        });

        modelBuilder.Entity<PhotoAnalysisCache>(entity =>
        {
            entity.HasKey(e => e.PhotoId);
            entity.HasIndex(e => e.CreatedAt);

            entity.Property(e => e.AnalysisJson)
                .HasColumnType("jsonb");
        });
    }
}
