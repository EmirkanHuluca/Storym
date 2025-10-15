using MediatR;
using Storym.Application.Abstactions;
using Storym.Application.Abstractions;

namespace Storym.Application.Users.Commands;

public sealed record UnfollowUserCommand(string TargetUserId) : IRequest<bool>;

public sealed class UnfollowUserHandler : IRequestHandler<UnfollowUserCommand, bool>
{
    private readonly ICurrentUser _current;
    private readonly IFollowRepository _follows;

    public UnfollowUserHandler(ICurrentUser current, IFollowRepository follows)
    { _current = current; _follows = follows; }

    public Task<bool> Handle(UnfollowUserCommand r, CancellationToken ct) =>
        string.IsNullOrWhiteSpace(r.TargetUserId) || r.TargetUserId == _current.UserId
            ? Task.FromResult(false)
            : _follows.UnfollowAsync(_current.UserId, r.TargetUserId, ct);
}
