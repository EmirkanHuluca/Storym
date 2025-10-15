using MediatR;
using Storym.Application.Abstactions;
using Storym.Application.Abstractions;

namespace Storym.Application.Users.Commands;

public sealed record FollowUserCommand(string TargetUserId) : IRequest<bool>;

public sealed class FollowUserHandler : IRequestHandler<FollowUserCommand, bool>
{
    private readonly ICurrentUser _current;
    private readonly IFollowRepository _follows;

    public FollowUserHandler(ICurrentUser current, IFollowRepository follows)
    { _current = current; _follows = follows; }

    public async Task<bool> Handle(FollowUserCommand r, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(r.TargetUserId) || r.TargetUserId == _current.UserId)
            return false; // cannot follow self

        return await _follows.FollowAsync(_current.UserId, r.TargetUserId, ct);
    }
}