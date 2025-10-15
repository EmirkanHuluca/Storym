using MediatR;
using Storym.Application.Abstactions;
using Storym.Domain.Diary;

namespace Storym.Application.Diary.Queries;

public sealed record GetUserPostsQuery(string TargetUserId) : IRequest<List<DiaryEntryDto>>;

public sealed class GetUserPostsHandler : IRequestHandler<GetUserPostsQuery, List<DiaryEntryDto>>
{
    private readonly IDiaryEntryRepository _repo;
    private readonly ICurrentUser _current;
    private readonly IUserReadService _users;
    private readonly IFollowReadService _follows;   // <-- add

    public GetUserPostsHandler(IDiaryEntryRepository repo, ICurrentUser current, IUserReadService users, IFollowReadService follows)
    { _repo = repo; _current = current; _users = users; _follows = follows; }

    public async Task<List<DiaryEntryDto>> Handle(GetUserPostsQuery r, CancellationToken ct)
    {
        var me = _current.UserId;
        var isMe = !string.IsNullOrEmpty(me) && me == r.TargetUserId;

        var entries = await _repo.GetByUserAsync(r.TargetUserId, includeHidden: isMe, ct);
        if (!isMe) entries = entries.Where(e => !e.IsHidden).ToList();

        var owner = await _users.GetUserAsync(r.TargetUserId, ct) ?? new UserMini(r.TargetUserId, "Unknown", "", null);

        var followingSet = string.IsNullOrEmpty(me)
            ? new HashSet<string>()
            : await _follows.FollowingSetAsync(me, new[] { r.TargetUserId }, ct);

        return entries.Select(e => new DiaryEntryDto(
            Id: e.Id,
            Title: e.Title,
            Summary: e.Summary,
            Content: e.Content,
            Date: e.Date,
            IsHidden: e.IsHidden,
            UserId: e.UserId,
            ImageUrls: e.Images.Select(i => i.Path).ToList(),
            LikeCount: e.Likes.Count,
            LikedByMe: !string.IsNullOrEmpty(me) && e.Likes.Any(l => l.UserId == me),
            OwnerNick: owner.Nick,
            OwnerEmail: owner.Email,
            OwnerAvatarUrl: owner.AvatarUrl,
            OwnerFollowing: !string.IsNullOrEmpty(me) && followingSet.Contains(e.UserId) // <-- NEW
        )).ToList();
    }
}
