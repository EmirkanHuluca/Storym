
namespace Storym.Application.Abstactions;


public record UserMini(string Id, string Nick, string Email, string? AvatarUrl);

public record UserListItem(string Id, string Nick, string Email, string? AvatarUrl, bool Following);

public interface IUserReadService
{
    Task<Dictionary<string, UserMini>> GetUsersAsync(IEnumerable<string> userIds, CancellationToken ct);
    Task<UserMini?> GetUserAsync(string userId, CancellationToken ct);
    Task<List<UserListItem>> SearchAsync(
        string? query, int take, string? excludeUserId, string? currentUserId, CancellationToken ct);
}