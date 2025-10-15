using MediatR;
using Storym.Application.Abstractions;

namespace Storym.Application.Diary.Queries;

public record GetMineQuery() : IRequest<List<Storym.Application.Diary.DiaryEntryDto>>;

public sealed class GetMineHandler : IRequestHandler<GetMineQuery, List<Storym.Application.Diary.DiaryEntryDto>>
{
    private readonly IDiaryEntryRepository _repo;
    private readonly ICurrentUser _user;
    public GetMineHandler(IDiaryEntryRepository repo, ICurrentUser user)
    { _repo = repo; _user = user; }

    public async Task<List<Storym.Application.Diary.DiaryEntryDto>> Handle(GetMineQuery r, CancellationToken ct)
    {
        var list = await _repo.GetByUserAsync(_user.UserId, includeHidden: true, ct);
        return list.Select(e => new Storym.Application.Diary.DiaryEntryDto(
            e.Id, e.Title, e.Summary, e.Content, e.Date, e.IsHidden, e.UserId,
            e.Images.Select(i => i.Path).ToList(), e.Likes.Count, e.Likes.Any(l => l.UserId == _user.UserId)
        )).ToList();
    }
}
