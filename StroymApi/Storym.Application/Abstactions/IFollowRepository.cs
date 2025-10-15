namespace Storym.Application.Abstractions;

public interface IFollowRepository
{
    Task<bool> IsFollowingAsync(string followerId, string followeeId, CancellationToken ct);
    Task<bool> FollowAsync(string followerId, string followeeId, CancellationToken ct);     // returns true if created
    Task<bool> UnfollowAsync(string followerId, string followeeId, CancellationToken ct);   // returns true if deleted
    Task<(int Followers, int Following)> GetCountsAsync(string userId, CancellationToken ct);
}
