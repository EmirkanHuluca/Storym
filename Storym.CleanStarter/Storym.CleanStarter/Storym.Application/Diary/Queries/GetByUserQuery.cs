using MediatR;
using Storym.Application.Abstractions;

namespace Storym.Application.Diary.Queries;

public record GetByUserQuery(string UserId) : IRequest<List<Storym.Application.Diary.DiaryEntryDto>>;

public sealed class GetByUserHandler : IRequestHandler<GetByUserQuery, List<Storym.Application.Diary.DiaryEntryDto>>
{
    private readonly IDiaryEntryRepository _repo;
    private readonly ICurrentUser _user;
    public GetByUserHandler(IDiaryEntryRepository repo, ICurrentUser user)
    { _repo = repo; _user = user; }

    public async Task<List<Storym.Application.Diary.DiaryEntryDto>> Handle(GetByUserQuery r, CancellationToken ct)
    {
        var list = await _repo.GetByUserAsync(r.UserId, includeHidden: false, ct);
        return list.Select(e => new Storym.Application.Diary.DiaryEntryDto(
            e.Id, e.Title, e.Summary, e.Content, e.Date, e.IsHidden, e.UserId,
            e.Images.Select(i => i.Path).ToList(), e.Likes.Count, e.Likes.Any(l => l.UserId == _user.UserId)
        )).ToList();
    }
}
