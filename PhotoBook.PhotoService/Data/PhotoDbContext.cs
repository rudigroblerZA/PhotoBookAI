using Microsoft.EntityFrameworkCore;
using PhotoBook.Shared.Models;

namespace PhotoBook.PhotoService.Data;

public class PhotoDbContext : DbContext
{
    public PhotoDbContext(DbContextOptions<PhotoDbContext> options)
        : base(options) { }

    public DbSet<PhotoMetadata> Photos { get; set; }
    public DbSet<PhotoAlbum> Albums { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PhotoMetadata>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            //entity.HasIndex(e => e.UploadedAt);
            //entity.HasIndex(e => e.DateTaken);

            entity.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.BlobUrl)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(e => e.ThumbnailUrl)
                .HasMaxLength(1000);

            entity.Property(e => e.Location)
                .HasMaxLength(500);
        });

        modelBuilder.Entity<PhotoAlbum>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
        });
    }
}

public class PhotoAlbum
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    //public DateTime CreatedAt { get; set; }
    public List<Guid> PhotoIds { get; set; } = new();
}
