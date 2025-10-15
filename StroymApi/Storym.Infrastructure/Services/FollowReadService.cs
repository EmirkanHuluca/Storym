using Microsoft.EntityFrameworkCore;
using Storym.Application.Abstactions;
using Storym.Infrastructure.Persistence;

namespace Storym.Infrastructure.Services;

public sealed class FollowReadService : IFollowReadService
{
    private readonly AppDbContext _db;
    public FollowReadService(AppDbContext db) => _db = db;

    public async Task<PagedResult<UserListItem>> GetFollowersAsync(string targetUserId, string? currentUserId, int page, int pageSize, CancellationToken ct)
    {
        var baseQ = _db.Follows.AsNoTracking().Where(f => f.FolloweeId == targetUserId);
        var total = await baseQ.CountAsync(ct);

        var q = baseQ
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => f.FollowerId);

        var list = await _db.Users
            .Where(u => q.Contains(u.Id))
            .Select(u => new {
                u.Id,
                u.UserNick,
                u.Email,
                u.ProfilePicturePath,
                Following = currentUserId != null &&
                    _db.Follows.Any(ff => ff.FollowerId == currentUserId && ff.FolloweeId == u.Id)
            })
            .ToListAsync(ct);

        var items = list.Select(u => new UserListItem(u.Id, u.UserNick ?? "", u.Email ?? "", u.ProfilePicturePath, u.Following)).ToList();
        return new PagedResult<UserListItem>(items, total, page, pageSize);
    }

    public async Task<PagedResult<UserListItem>> GetFollowingAsync(string userId, string? currentUserId, int page, int pageSize, CancellationToken ct)
    {
        var baseQ = _db.Follows.AsNoTracking().Where(f => f.FollowerId == userId);
        var total = await baseQ.CountAsync(ct);

        var q = baseQ
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => f.FolloweeId);

        var list = await _db.Users
            .Where(u => q.Contains(u.Id))
            .Select(u => new {
                u.Id,
                u.UserNick,
                u.Email,
                u.ProfilePicturePath,
                Following = currentUserId != null &&
                    _db.Follows.Any(ff => ff.FollowerId == currentUserId && ff.FolloweeId == u.Id)
            })
            .ToListAsync(ct);

        var items = list.Select(u => new UserListItem(u.Id, u.UserNick ?? "", u.Email ?? "", u.ProfilePicturePath, u.Following)).ToList();
        return new PagedResult<UserListItem>(items, total, page, pageSize);
    }

    public async Task<HashSet<string>> FollowingSetAsync(string currentUserId, IEnumerable<string> targetIds, CancellationToken ct)
    {
        var ids = targetIds.Distinct().ToArray();
        if (ids.Length == 0) return new HashSet<string>();
        var list = await _db.Follows
            .AsNoTracking()
            .Where(f => f.FollowerId == currentUserId && ids.Contains(f.FolloweeId))
            .Select(f => f.FolloweeId)
            .ToListAsync(ct);
        return list.ToHashSet();
    }
}
