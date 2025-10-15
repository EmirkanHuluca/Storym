using MediatR;
using Storym.Application.Abstactions;


namespace Storym.Application.Users.Queries;

public sealed record GetFollowingPageQuery(string UserId, int Page, int PageSize) : IRequest<PagedResult<UserListItem>>;

public sealed class GetFollowingPageHandler : IRequestHandler<GetFollowingPageQuery, PagedResult<UserListItem>>
{
    private readonly ICurrentUser _current;
    private readonly IFollowReadService _read;
    public GetFollowingPageHandler(ICurrentUser current, IFollowReadService read) { _current = current; _read = read; }

    public Task<PagedResult<UserListItem>> Handle(GetFollowingPageQuery r, CancellationToken ct) =>
        _read.GetFollowingAsync(r.UserId, _current.UserId, r.Page, r.PageSize, ct);
}
