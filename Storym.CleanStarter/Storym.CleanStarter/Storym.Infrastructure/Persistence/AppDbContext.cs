using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Storym.Domain.Diary;
using Storym.Infrastructure.Auth;

namespace Storym.Infrastructure.Persistence;

public sealed class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<DiaryEntry> DiaryEntries => Set<DiaryEntry>();
    public DbSet<DiaryImage> DiaryImages => Set<DiaryImage>();
    public DbSet<Like> Likes => Set<Like>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<DiaryEntry>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Summary).HasMaxLength(2000).IsRequired();
            e.Property(x => x.Content).IsRequired();
            e.HasMany(x => x.Images).WithOne().HasForeignKey(i => i.DiaryEntryId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Likes).WithOne().HasForeignKey(l => l.DiaryEntryId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Like>(e =>
        {
            e.HasIndex(l => new { l.DiaryEntryId, l.UserId }).IsUnique();
        });
    }
}
