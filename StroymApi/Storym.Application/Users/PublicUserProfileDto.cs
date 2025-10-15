namespace Storym.Application.Users;

public sealed record PublicUserProfileDto(
    string Id,
    string Nick,
    string Email,
    string? AvatarUrl,
    int EntryCount,        // visible entry count (public unless self)
    int FollowerCount,
    int FollowingCount,
    bool IsMe,
    bool Following         // whether current user follows this profile
);
