using MediatR;
using Storym.Application.Abstactions;

namespace Storym.Application.Users.Queries;

public sealed record GetFollowersPageQuery(string TargetUserId, int Page, int PageSize) : IRequest<PagedResult<UserListItem>>;

public sealed class GetFollowersPageHandler : IRequestHandler<GetFollowersPageQuery, PagedResult<UserListItem>>
{
    private readonly ICurrentUser _current;
    private readonly IFollowReadService _read;
    public GetFollowersPageHandler(ICurrentUser current, IFollowReadService read) { _current = current; _read = read; }

    public Task<PagedResult<UserListItem>> Handle(GetFollowersPageQuery r, CancellationToken ct) =>
        _read.GetFollowersAsync(r.TargetUserId, _current.UserId, r.Page, r.PageSize, ct);
}
