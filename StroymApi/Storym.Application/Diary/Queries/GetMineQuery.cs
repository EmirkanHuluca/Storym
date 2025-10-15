using MediatR;
using Storym.Application.Abstactions;

namespace Storym.Application.Diary.Queries;

public record GetMineQuery() : IRequest<List<Storym.Application.Diary.DiaryEntryDto>>;

public sealed class GetMineHandler : IRequestHandler<GetMineQuery, List<Storym.Application.Diary.DiaryEntryDto>>
{
    private readonly IDiaryEntryRepository _repo;
    private readonly ICurrentUser _user;
    private readonly IUserReadService _userReadService;

    public GetMineHandler(IDiaryEntryRepository repo, ICurrentUser user, IUserReadService userReadService)
    {
        _repo = repo; _user = user;
        _userReadService = userReadService;
    }

    public async Task<List<Storym.Application.Diary.DiaryEntryDto>> Handle(GetMineQuery r, CancellationToken ct)
    {
        var list = await _repo.GetByUserAsync(_user.UserId, includeHidden: true, ct);
        await _repo.SaveChangesAsync(ct);
        var owner = await _userReadService.GetUserAsync(_user.UserId, ct)
                   ?? new UserMini(_user.UserId, "Unknown", "", null);

        var myEntries = await _repo.GetByUserAsync(_user.UserId, includeHidden: true, ct);
        var entryCount = myEntries.Count;

        return list.Select(e => new Storym.Application.Diary.DiaryEntryDto(
            e.Id, e.Title, e.Summary, e.Content, e.Date, e.IsHidden, e.UserId,
            e.Images.Select(i => i.Path).ToList(), e.Likes.Count, e.Likes.Any(l => l.UserId == _user.UserId), owner.Nick, owner.Email, owner.AvatarUrl, 
            OwnerFollowing: !string.IsNullOrEmpty(_user.UserId)
        )).ToList();
    }
}
