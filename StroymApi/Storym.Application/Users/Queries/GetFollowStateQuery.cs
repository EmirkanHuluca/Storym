using MediatR;
using Storym.Application.Abstactions;
using Storym.Application.Abstractions;

namespace Storym.Application.Users.Queries;

public sealed record GetFollowStateQuery(string TargetUserId) : IRequest<bool>;

public sealed class GetFollowStateHandler : IRequestHandler<GetFollowStateQuery, bool>
{
    private readonly ICurrentUser _current;
    private readonly IFollowRepository _follows;

    public GetFollowStateHandler(ICurrentUser current, IFollowRepository follows)
    { _current = current; _follows = follows; }

    public Task<bool> Handle(GetFollowStateQuery r, CancellationToken ct) =>
        string.IsNullOrWhiteSpace(r.TargetUserId) || r.TargetUserId == _current.UserId
            ? Task.FromResult(false)
            : _follows.IsFollowingAsync(_current.UserId, r.TargetUserId, ct);
}
