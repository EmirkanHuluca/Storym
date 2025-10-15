using Microsoft.EntityFrameworkCore;
using Storym.Application.Abstactions;
using Storym.Infrastructure.Persistence;

namespace Storym.Infrastructure.Services;

public sealed class UserReadService : IUserReadService
{
    private readonly AppDbContext _db;
    public UserReadService(AppDbContext db) => _db = db;

    public async Task<Dictionary<string, UserMini>> GetUsersAsync(IEnumerable<string> userIds, CancellationToken ct)
    {
        var ids = userIds.Distinct().ToArray();
        var users = await _db.Users
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.UserNick, u.Email, u.ProfilePicturePath })
            .ToListAsync(ct);

        return users.ToDictionary(
            u => u.Id,
            u => new UserMini(u.Id, u.UserNick ?? "", u.Email ?? "", u.ProfilePicturePath)
        );
    }

    public async Task<UserMini?> GetUserAsync(string userId, CancellationToken ct)
    {
        var u = await _db.Users
            .Where(x => x.Id == userId)
            .Select(x => new { x.Id, x.UserNick, x.Email, x.ProfilePicturePath })
            .FirstOrDefaultAsync(ct);

        return u is null ? null : new UserMini(u.Id, u.UserNick ?? "", u.Email ?? "", u.ProfilePicturePath);
    }

    public async Task<List<UserListItem>> SearchAsync(
        string? query, int take, string? excludeUserId, string? currentUserId, CancellationToken ct)
    {
        var q = _db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(excludeUserId))
            q = q.Where(u => u.Id != excludeUserId);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var s = query.Trim();
            q = q.Where(u =>
                (u.UserNick != null && u.UserNick.Contains(s)) ||
                (u.Email != null && u.Email.Contains(s)));
        }

        // Correlated subquery: is 'currentUserId' following this user?
        var list = await q
            .OrderBy(u => u.UserNick)
            .Take(Math.Max(1, take))
            .Select(u => new
            {
                u.Id,
                u.UserNick,
                u.Email,
                u.ProfilePicturePath,
                Following = currentUserId != null &&
                    _db.Follows.Any(f => f.FollowerId == currentUserId && f.FolloweeId == u.Id)
            })
            .ToListAsync(ct);

        return list.Select(u =>
            new UserListItem(u.Id, u.UserNick ?? "", u.Email ?? "", u.ProfilePicturePath, u.Following)
        ).ToList();
    }
}