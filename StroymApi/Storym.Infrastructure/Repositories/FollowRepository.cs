using Microsoft.EntityFrameworkCore;
using Storym.Application.Abstractions;
using Storym.Domain.Social;
using Storym.Infrastructure.Persistence;

namespace Storym.Infrastructure.Repositories;

public sealed class FollowRepository : IFollowRepository
{
    private readonly AppDbContext _db;
    public FollowRepository(AppDbContext db) => _db = db;

    public Task<bool> IsFollowingAsync(string followerId, string followeeId, CancellationToken ct) =>
        _db.Follows.AnyAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId, ct);

    public async Task<bool> FollowAsync(string followerId, string followeeId, CancellationToken ct)
    {
        if (await IsFollowingAsync(followerId, followeeId, ct)) return false;
        _db.Follows.Add(new Follow(followerId, followeeId));
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> UnfollowAsync(string followerId, string followeeId, CancellationToken ct)
    {
        var entity = await _db.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId, ct);
        if (entity is null) return false;
        _db.Follows.Remove(entity);
        try
        {
            await _db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            // someone else removed it; treat as "already unfollowed"
            return false;
        }
    }

    public async Task<(int Followers, int Following)> GetCountsAsync(string userId, CancellationToken ct)
    {
        var followers = await _db.Follows.CountAsync(f => f.FolloweeId == userId, ct);
        var following = await _db.Follows.CountAsync(f => f.FollowerId == userId, ct);
        return (followers, following);
    }
}
