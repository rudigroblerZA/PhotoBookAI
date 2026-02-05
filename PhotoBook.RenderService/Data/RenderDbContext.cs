using Microsoft.EntityFrameworkCore;
using PhotoBook.RenderService.Models;
using static System.Net.WebRequestMethods;

namespace PhotoBook.RenderService.Data;

public class RenderDbContext : DbContext
{
    public RenderDbContext(DbContextOptions<RenderDbContext> options)
        : base(options) { }

    public DbSet<RenderJob> RenderJobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.Entity<RenderJob>(entity =>
        //{
        //    entity.HasKey(e => https://url.za.m.mimecastprotect.com/s/n4cSCnZmMAsLQGQXSmtjHJiZ9S?domain=e.id);
        //    entity.HasIndex(e => e.PhotoBookId);
        //    entity.HasIndex(e => e.UserId);
        //    entity.HasIndex(e => e.Status);
        //    entity.HasIndex(e => e.CreatedAt);

        //https://url.za.m.mimecastprotect.com/s/slu9Ck5jJxh0ZnZkSVhGHGPaSV?domain=entity.property(e => e.Status).HasMaxLength(50);
        //https://url.za.m.mimecastprotect.com/s/slu9Ck5jJxh0ZnZkSVhGHGPaSV?domain=entity.property(e => e.PdfBlobUrl).HasMaxLength(1000);
        //https://url.za.m.mimecastprotect.com/s/slu9Ck5jJxh0ZnZkSVhGHGPaSV?domain=entity.property(e => e.ErrorMessage).HasMaxLength(2000);
        //});
    }
}
