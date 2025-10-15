namespace Storym.Domain.Social;

public sealed class Follow
{
    public string FollowerId { get; private set; } = default!; // who follows
    public string FolloweeId { get; private set; } = default!; // who is followed
    public DateTime CreatedAt { get; private set; }

    private Follow() { } // EF
    public Follow(string followerId, string followeeId)
    {
        if (string.IsNullOrWhiteSpace(followerId)) throw new ArgumentException(nameof(followerId));
        if (string.IsNullOrWhiteSpace(followeeId)) throw new ArgumentException(nameof(followeeId));
        if (followerId == followeeId) throw new InvalidOperationException("You cannot follow yourself.");

        FollowerId = followerId;
        FolloweeId = followeeId;
        CreatedAt = DateTime.UtcNow;
    }
}
