using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PhotoBook.Shared.Models;

namespace PhotoBook.DesignService.Data;

public class DesignDbContext : DbContext
{
    public DesignDbContext(DbContextOptions<DesignDbContext> options)
        : base(options) { }

    public DbSet<PhotoBook.Shared.Models.PhotoBook> PhotoBooks { get; set; }
    public DbSet<Theme> Themes { get; set; }
    public DbSet<DesignTemplate> Templates { get; set; }
    public DbSet<PageSnapshot> PageSnapshots { get; set; }

    // Use a deterministic timestamp for seeded data to avoid model changes each build
    private static readonly DateTime SeedCreatedAtUtc = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PhotoBook.Shared.Models.PhotoBook>(entity =>
        {
            entity.HasKey(pb => pb.Id);
            entity.HasIndex(pb => pb.UserId);
            entity.HasIndex(pb => pb.Status);
            //entity.HasIndex(pb => pb.CreatedAt);

            entity.Property(pb => pb.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(pb => pb.Theme)
                .HasMaxLength(100);

            // Store Pages as JSON
            entity.Property(pb => pb.Pages)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<BookPage>>(v, (JsonSerializerOptions)null) ?? new List<BookPage>()
                )
                .HasColumnType("jsonb");
        });

        modelBuilder.Entity<Theme>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => t.Name).IsUnique();

            entity.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);
        });

        modelBuilder.Entity<DesignTemplate>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => t.Category);

            entity.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(t => t.LayoutJson)
                .HasColumnType("jsonb");
        });

        modelBuilder.Entity<PageSnapshot>(entity =>
        {
            entity.HasKey(ps => ps.Id);
            entity.HasIndex(ps => new { ps.PhotoBookId, ps.PageNumber });
            entity.HasIndex(ps => ps.CreatedAt);
        });

        // Seed default themes
        SeedThemes(modelBuilder);

        // Seed default templates
        SeedTemplates(modelBuilder);
    }

    private void SeedThemes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Theme>().HasData(
            new Theme
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Classic",
                Description = "Timeless elegance with clean lines",
                PrimaryColor = "#2C3E50",
                SecondaryColor = "#ECF0F1",
                AccentColor = "#E74C3C",
                FontFamily = "Georgia",
                FontSize = 14,
                BorderStyle = "simple",
                BorderWidth = 2,
                BorderColor = "#2C3E50",
                BackgroundColor = "#FFFFFF",
                IsActive = true,
                //CreatedAt = SeedCreatedAtUtc
            },
            new Theme
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Modern",
                Description = "Bold and contemporary design",
                PrimaryColor = "#000000",
                SecondaryColor = "#FFFFFF",
                AccentColor = "#FFD700",
                FontFamily = "Helvetica",
                FontSize = 16,
                BorderStyle = "none",
                BorderWidth = 0,
                BorderColor = "#000000",
                BackgroundColor = "#FFFFFF",
                IsActive = true,
                //CreatedAt = SeedCreatedAtUtc
            },
            new Theme
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Vintage",
                Description = "Nostalgic charm with warm tones",
                PrimaryColor = "#8B4513",
                SecondaryColor = "#F5DEB3",
                AccentColor = "#CD853F",
                FontFamily = "Times New Roman",
                FontSize = 13,
                BorderStyle = "ornate",
                BorderWidth = 3,
                BorderColor = "#8B4513",
                BackgroundColor = "#FFF8DC",
                IsActive = true,
                //CreatedAt = SeedCreatedAtUtc
            },
            new Theme
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Minimalist",
                Description = "Clean, simple, and sophisticated",
                PrimaryColor = "#333333",
                SecondaryColor = "#F9F9F9",
                AccentColor = "#999999",
                FontFamily = "Arial",
                FontSize = 12,
                BorderStyle = "none",
                BorderWidth = 0,
                BorderColor = "#333333",
                BackgroundColor = "#FFFFFF",
                IsActive = true,
                //CreatedAt = SeedCreatedAtUtc
            },
            new Theme
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Name = "Romantic",
                Description = "Soft pastels and elegant touches",
                PrimaryColor = "#C71585",
                SecondaryColor = "#FFE4E1",
                AccentColor = "#FF69B4",
                FontFamily = "Brush Script MT",
                FontSize = 15,
                BorderStyle = "rounded",
                BorderWidth = 2,
                BorderColor = "#C71585",
                BackgroundColor = "#FFF0F5",
                IsActive = true,
                //CreatedAt = SeedCreatedAtUtc
            }
        );
    }

    private void SeedTemplates(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DesignTemplate>().HasData(
            new DesignTemplate
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Single Hero",
                Description = "One large photo per page",
                Category = "Simple",
                PhotoSlots = 1,
                LayoutJson = """{"slots":[{"x":0.1,"y":0.1,"width":0.8,"height":0.8}]}""",
                PreviewUrl = "",
                IsActive = true,
                CreatedAt = SeedCreatedAtUtc
            },
            new DesignTemplate
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Name = "Side by Side",
                Description = "Two photos side by side",
                Category = "Simple",
                PhotoSlots = 2,
                LayoutJson = """{"slots":[{"x":0.05,"y":0.1,"width":0.4,"height":0.8},{"x":0.55,"y":0.1,"width":0.4,"height":0.8}]}""",
                PreviewUrl = "",
                IsActive = true,
                CreatedAt = SeedCreatedAtUtc
            },
            new DesignTemplate
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Name = "Grid 4",
                Description = "Four photos in a grid",
                Category = "Grid",
                PhotoSlots = 4,
                LayoutJson = """{"slots":[{"x":0.05,"y":0.05,"width":0.4,"height":0.4},{"x":0.55,"y":0.05,"width":0.4,"height":0.4},{"x":0.05,"y":0.55,"width":0.4,"height":0.4},{"x":0.55,"y":0.55,"width":0.4,"height":0.4}]}""",
                PreviewUrl = "",
                IsActive = true,
                CreatedAt = SeedCreatedAtUtc
            },
            new DesignTemplate
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                Name = "Feature + 2",
                Description = "One large photo with two smaller ones",
                Category = "Mixed",
                PhotoSlots = 3,
                LayoutJson = """{"slots":[{"x":0.05,"y":0.05,"width":0.6,"height":0.9},{"x":0.7,"y":0.05,"width":0.25,"height":0.4},{"x":0.7,"y":0.55,"width":0.25,"height":0.4}]}""",
                PreviewUrl = "",
                IsActive = true,
                CreatedAt = SeedCreatedAtUtc
            }
        );
    }
}

public class Theme
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PrimaryColor { get; set; } = "#000000";
    public string SecondaryColor { get; set; } = "#FFFFFF";
    public string AccentColor { get; set; } = "#FF0000";
    public string FontFamily { get; set; } = "Arial";
    public int FontSize { get; set; } = 14;
    public string BorderStyle { get; set; } = "none";
    public int BorderWidth { get; set; } = 0;
    public string BorderColor { get; set; } = "#000000";
    public string BackgroundColor { get; set; } = "#FFFFFF";
    public bool IsActive { get; set; } = true;
    //public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class DesignTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Simple, Grid, Mixed, etc.
    public int PhotoSlots { get; set; }
    public string LayoutJson { get; set; } = string.Empty;
    public string PreviewUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class PageSnapshot
{
    public Guid Id { get; set; }
    public Guid PhotoBookId { get; set; }
    public int PageNumber { get; set; }
    public string SnapshotUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
