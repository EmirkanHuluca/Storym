using MediatR;
using Storym.Application.Abstactions;

namespace Storym.Application.Diary.Queries;

public record GetFeedQuery() : IRequest<List<Storym.Application.Diary.DiaryEntryDto>>;

public sealed class GetFeedHandler : IRequestHandler<GetFeedQuery, List<Storym.Application.Diary.DiaryEntryDto>>
{
    private readonly IDiaryEntryRepository _repo;
    private readonly IUserReadService _users;
    private readonly ICurrentUser _current;
    private readonly IFollowReadService _follows;   // <-- add

    public GetFeedHandler(IDiaryEntryRepository repo, IUserReadService users, ICurrentUser current, IFollowReadService follows)
    { _repo = repo; _users = users; _current = current; _follows = follows; }

    public async Task<List<DiaryEntryDto>> Handle(GetFeedQuery r, CancellationToken ct)
    {
        var me = _current.UserId;
        var list = await _repo.GetFeedAsync(me, ct);
        var ownerIds = list.Select(e => e.UserId).Distinct().ToArray();
        var owners = await _users.GetUsersAsync(ownerIds, ct);
        var followingSet = string.IsNullOrEmpty(me)
            ? new HashSet<string>()
            : await _follows.FollowingSetAsync(me, ownerIds, ct);

        return list.Select(e =>
        {
            var o = owners[e.UserId];
            return new DiaryEntryDto(
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
                OwnerNick: o.Nick,
                OwnerEmail: o.Email,
                OwnerAvatarUrl: o.AvatarUrl,
                OwnerFollowing: !string.IsNullOrEmpty(me) && followingSet.Contains(e.UserId) // <-- NEW
            );
        }).ToList();
    }
}
