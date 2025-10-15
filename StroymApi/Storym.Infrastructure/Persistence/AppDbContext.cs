using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Storym.Domain.Diary;
using Storym.Domain.Social;
using Storym.Infrastructure.Auth;

namespace Storym.Infrastructure.Persistence;

public sealed class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<DiaryEntry> DiaryEntries => Set<DiaryEntry>();
    public DbSet<DiaryImage> DiaryImages => Set<DiaryImage>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<Follow> Follows => Set<Follow>();

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
        b.Entity<Follow>(e =>
        {
            e.ToTable("Follows");

            // composite PK ensures 1 unique follow per pair
            e.HasKey(f => new { f.FollowerId, f.FolloweeId });

            e.Property(f => f.FollowerId).HasMaxLength(450);
            e.Property(f => f.FolloweeId).HasMaxLength(450);
            e.Property(f => f.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            e.HasIndex(f => f.FollowerId);
            e.HasIndex(f => f.FolloweeId);

            // Optional but nice: real FKs to AspNetUsers
            e.HasOne<ApplicationUser>()
             .WithMany()
             .HasForeignKey(f => f.FollowerId)
             .OnDelete(DeleteBehavior.NoAction);

            e.HasOne<ApplicationUser>()
             .WithMany()
             .HasForeignKey(f => f.FolloweeId)
             .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
