using MediatR;
using Storym.Application.Abstactions;

namespace Storym.Application.Diary.Queries;

public record GetDetailsQuery(int Id) : IRequest<Storym.Application.Diary.DiaryEntryDto>;

public sealed class GetDetailsHandler : IRequestHandler<GetDetailsQuery, Storym.Application.Diary.DiaryEntryDto>
{
    private readonly IDiaryEntryRepository _repo;
    private readonly ICurrentUser _user;
    private readonly IUserReadService _userReadService;
    public GetDetailsHandler(IDiaryEntryRepository repo, ICurrentUser user,IUserReadService userReadService)
    { _repo = repo; _user = user; _userReadService = userReadService; }

    public async Task<Storym.Application.Diary.DiaryEntryDto> Handle(GetDetailsQuery r, CancellationToken ct)
    {
        var e = await _repo.GetAsync(r.Id, ct) ?? throw new KeyNotFoundException("Diary entry not found");
        var owner = await _userReadService.GetUserAsync(_user.UserId, ct)
                   ?? new UserMini(_user.UserId, "Unknown", "", null);
        return new Storym.Application.Diary.DiaryEntryDto(
            e.Id, e.Title, e.Summary, e.Content, e.Date, e.IsHidden, e.UserId,
            e.Images.Select(i => i.Path).ToList(), e.Likes.Count, e.Likes.Any(l => l.UserId == _user.UserId),owner.Nick,owner.Email,owner.AvatarUrl,
            OwnerFollowing: !string.IsNullOrEmpty(_user.UserId));
    }
}
