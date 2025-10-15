using MediatR;
using Storym.Application.Abstractions;
using Storym.Domain.Diary;

namespace Storym.Application.Diary.Commands;

public record CreateDiaryEntryCommand(
    string Title,
    string Summary,
    string Content,
    DateTime Date,
    bool IsHidden,
    IReadOnlyList<(string FileName, Stream Content)> Files
) : IRequest<Storym.Application.Diary.DiaryEntryDto>;

public sealed class CreateDiaryEntryHandler : IRequestHandler<CreateDiaryEntryCommand, Storym.Application.Diary.DiaryEntryDto>
{
    private readonly IDiaryEntryRepository _repo;
    private readonly IFileStorage _files;
    private readonly ICurrentUser _user;

    public CreateDiaryEntryHandler(IDiaryEntryRepository repo, IFileStorage files, ICurrentUser user)
    { _repo = repo; _files = files; _user = user; }

    public async Task<Storym.Application.Diary.DiaryEntryDto> Handle(CreateDiaryEntryCommand r, CancellationToken ct)
    {
        var entry = new DiaryEntry(_user.UserId, r.Title, r.Summary, r.Content, r.Date, r.IsHidden);

        foreach (var (name, stream) in r.Files)
        {
            var path = await _files.SaveAsync(stream, name, ct);
            entry.AddImage(path);
        }

        await _repo.AddAsync(entry, ct);
        await _repo.SaveChangesAsync(ct);

        return new Storym.Application.Diary.DiaryEntryDto(
            entry.Id, entry.Title, entry.Summary, entry.Content, entry.Date, entry.IsHidden, entry.UserId,
            entry.Images.Select(i => i.Path).ToList(), entry.Likes.Count, false);
    }
}
