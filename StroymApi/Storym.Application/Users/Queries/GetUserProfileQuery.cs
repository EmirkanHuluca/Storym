using MediatR;
using Storym.Application.Abstactions;
using Storym.Application.Abstractions;

namespace Storym.Application.Users.Queries;

public sealed record GetUserProfileQuery(string TargetUserId) : IRequest<PublicUserProfileDto>;

public sealed class GetUserProfileHandler : IRequestHandler<GetUserProfileQuery, PublicUserProfileDto>
{
    private readonly ICurrentUser _current;
    private readonly IUserReadService _users;
    private readonly IDiaryEntryRepository _entries;
    private readonly IFollowRepository _follows;

    public GetUserProfileHandler(ICurrentUser current, IUserReadService users,
                                 IDiaryEntryRepository entries, IFollowRepository follows)
    {
        _current = current; _users = users; _entries = entries; _follows = follows;
    }

    public async Task<PublicUserProfileDto> Handle(GetUserProfileQuery r, CancellationToken ct)
    {
        var meId = _current.UserId;
        var isMe = !string.IsNullOrEmpty(meId) && meId == r.TargetUserId;

        var u = await _users.GetUserAsync(r.TargetUserId, ct)
                ?? new UserMini(r.TargetUserId, "Unknown", "", null);

        // Entry count: if it’s me, include hidden; otherwise only public
        var list = await _entries.GetByUserAsync(r.TargetUserId, includeHidden: isMe, ct);
        var visibleCount = isMe ? list.Count : list.Count(e => !e.IsHidden);

        var (followers, following) = await _follows.GetCountsAsync(r.TargetUserId, ct);
        var iFollow = !string.IsNullOrEmpty(meId)
                      && meId != r.TargetUserId
                      && await _follows.IsFollowingAsync(meId, r.TargetUserId, ct);

        return new PublicUserProfileDto(
            Id: u.Id,
            Nick: u.Nick,
            Email: u.Email,
            AvatarUrl: u.AvatarUrl,
            EntryCount: visibleCount,
            FollowerCount: followers,
            FollowingCount: following,
            IsMe: isMe,
            Following: iFollow
        );
    }
}
