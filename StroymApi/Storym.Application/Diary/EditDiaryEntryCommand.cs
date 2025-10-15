using MediatR;
using Storym.Application.Abstactions;

namespace Storym.Application.Diary.Commands;

public record EditDiaryEntryCommand(
    int Id,
    string Title,
    string Summary,
    string Content,
    DateTime Date,
    bool IsHidden,
    IReadOnlyList<string> DeleteImages,
    IReadOnlyList<(string FileName, Stream Content)> NewFiles
) : IRequest<Storym.Application.Diary.DiaryEntryDto>;

public sealed class EditDiaryEntryHandler : IRequestHandler<EditDiaryEntryCommand, Storym.Application.Diary.DiaryEntryDto>
{
    private readonly IDiaryEntryRepository _repo;
    private readonly IFileStorage _files;
    private readonly ICurrentUser _user;
    private readonly IUserReadService _userReadService;

    public EditDiaryEntryHandler(IDiaryEntryRepository repo, IFileStorage files, ICurrentUser user, IUserReadService userReadService)
    {
        _repo = repo; _files = files; _user = user;
        _userReadService = userReadService;
    }

    public async Task<Storym.Application.Diary.DiaryEntryDto> Handle(EditDiaryEntryCommand r, CancellationToken ct)
    {
        var entry = await _repo.GetAsync(r.Id, ct) ?? throw new KeyNotFoundException("Diary entry not found");

        if (entry.UserId != _user.UserId && !_user.IsInRole("Admin"))
            throw new Common.Exceptions.ForbiddenAccessException();

        entry.Edit(r.Title, r.Summary, r.Content, r.Date, r.IsHidden);

        foreach (var del in r.DeleteImages)
            entry.RemoveImage(del);

        foreach (var (name, stream) in r.NewFiles)
        {
            var path = await _files.SaveAsync(stream, name, ct);
            entry.AddImage(path);
        }

        await _repo.SaveChangesAsync(ct);
        var owner = await _userReadService.GetUserAsync(_user.UserId, ct)
                   ?? new UserMini(_user.UserId, "Unknown", "", null);

        return new Storym.Application.Diary.DiaryEntryDto(
            entry.Id, entry.Title, entry.Summary, entry.Content, entry.Date, entry.IsHidden, entry.UserId,
            entry.Images.Select(i => i.Path).ToList(), entry.Likes.Count, false, owner.Nick, owner.Email, owner.AvatarUrl,
            OwnerFollowing: !string.IsNullOrEmpty(_user.UserId));
    }
}
