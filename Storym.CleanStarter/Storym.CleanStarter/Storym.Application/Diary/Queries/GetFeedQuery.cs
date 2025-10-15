using MediatR;
using Storym.Application.Abstractions;

namespace Storym.Application.Diary.Queries;

public record GetFeedQuery() : IRequest<List<Storym.Application.Diary.DiaryEntryDto>>;

public sealed class GetFeedHandler : IRequestHandler<GetFeedQuery, List<Storym.Application.Diary.DiaryEntryDto>>
{
    private readonly IDiaryEntryRepository _repo;
    private readonly ICurrentUser _user;
    public GetFeedHandler(IDiaryEntryRepository repo, ICurrentUser user)
    { _repo = repo; _user = user; }

    public async Task<List<Storym.Application.Diary.DiaryEntryDto>> Handle(GetFeedQuery r, CancellationToken ct)
    {
        var list = await _repo.GetFeedAsync(_user.UserId, ct);
        return list.Select(e => new Storym.Application.Diary.DiaryEntryDto(
            e.Id, e.Title, e.Summary, e.Content, e.Date, e.IsHidden, e.UserId,
            e.Images.Select(i => i.Path).ToList(), e.Likes.Count, e.Likes.Any(l => l.UserId == _user.UserId)
        )).ToList();
    }
}
