using MediatR;
using Storym.Application.Abstactions;
using Storym.Application.Abstractions;
using Storym.Application.Users;

namespace Storym.Application.Users.Queries;

public sealed record GetMyProfileQuery() : IRequest<UserProfileDto>;

public sealed class GetMyProfileHandler : IRequestHandler<GetMyProfileQuery, UserProfileDto>
{
    private readonly ICurrentUser _current;
    private readonly IUserReadService _users;
    private readonly IDiaryEntryRepository _entries;
    private readonly IFollowRepository _follows;

    public GetMyProfileHandler(ICurrentUser current, IUserReadService users, IDiaryEntryRepository entries,IFollowRepository follows)
    { _current = current; _users = users; _entries = entries; _follows = follows; }

    public async Task<UserProfileDto> Handle(GetMyProfileQuery request, CancellationToken ct)
    {
        var meId = _current.UserId;

        // Pull basic user fields (nick/email/avatar) via the port
        var me = await _users.GetUserAsync(meId, ct)
                 ?? new UserMini(meId, "Unknown", "", null);

        // Entry count (quick + simple; we’ll optimize later if needed)
        var myEntries = await _entries.GetByUserAsync(meId, includeHidden: true, ct);
        var entryCount = myEntries.Count;
        var (followers, following) = await _follows.GetCountsAsync(meId, ct);

        // Follower/Following counts will be 0 until we add the follow schema
        return new UserProfileDto(
            Id: me.Id,
            Nick: me.Nick,
            Email: me.Email,
            AvatarUrl: me.AvatarUrl,
            EntryCount: entryCount,
            FollowerCount: followers,
            FollowingCount: following
        );
    }
}
