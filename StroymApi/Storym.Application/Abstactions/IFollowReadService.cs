

namespace Storym.Application.Abstactions;

public interface IFollowReadService
{
    Task<PagedResult<UserListItem>> GetFollowersAsync(string targetUserId, string? currentUserId, int page, int pageSize, CancellationToken ct);
    Task<PagedResult<UserListItem>> GetFollowingAsync(string userId, string? currentUserId, int page, int pageSize, CancellationToken ct);
   
    //returns the subset of targetIds that currentUserId is following
    Task<HashSet<string>> FollowingSetAsync(string currentUserId, IEnumerable<string> targetIds, CancellationToken ct);


}
