namespace Storym.Application.Users;

public sealed record UserProfileDto(
    string Id,
    string Nick,
    string Email,
    string? AvatarUrl,
    int EntryCount,
    int FollowerCount,   // 0 for now — we’ll wire real follow data later
    int FollowingCount   // 0 for now — we’ll wire real follow data later
);
