using MediatR;
using Storym.Application.Abstractions;

namespace Storym.Application.Diary.Commands;

public record ToggleLikeCommand(int Id) : IRequest<int>; // returns like count

public sealed class ToggleLikeHandler : IRequestHandler<ToggleLikeCommand, int>
{
    private readonly IDiaryEntryRepository _repo;
    private readonly ICurrentUser _user;
    public ToggleLikeHandler(IDiaryEntryRepository repo, ICurrentUser user)
    { _repo = repo; _user = user; }

    public async Task<int> Handle(ToggleLikeCommand r, CancellationToken ct)
    {
        var entry = await _repo.GetAsync(r.Id, ct) ?? throw new KeyNotFoundException("Diary entry not found");
        entry.ToggleLike(_user.UserId);
        await _repo.SaveChangesAsync(ct);
        return entry.Likes.Count;
    }
}
