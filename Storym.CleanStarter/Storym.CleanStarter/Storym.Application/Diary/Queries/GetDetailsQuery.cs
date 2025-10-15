using MediatR;
using Storym.Application.Abstractions;

namespace Storym.Application.Diary.Queries;

public record GetDetailsQuery(int Id) : IRequest<Storym.Application.Diary.DiaryEntryDto>;

public sealed class GetDetailsHandler : IRequestHandler<GetDetailsQuery, Storym.Application.Diary.DiaryEntryDto>
{
    private readonly IDiaryEntryRepository _repo;
    private readonly ICurrentUser _user;
    public GetDetailsHandler(IDiaryEntryRepository repo, ICurrentUser user)
    { _repo = repo; _user = user; }

    public async Task<Storym.Application.Diary.DiaryEntryDto> Handle(GetDetailsQuery r, CancellationToken ct)
    {
        var e = await _repo.GetAsync(r.Id, ct) ?? throw new KeyNotFoundException("Diary entry not found");
        return new Storym.Application.Diary.DiaryEntryDto(
            e.Id, e.Title, e.Summary, e.Content, e.Date, e.IsHidden, e.UserId,
            e.Images.Select(i => i.Path).ToList(), e.Likes.Count, e.Likes.Any(l => l.UserId == _user.UserId));
    }
}
