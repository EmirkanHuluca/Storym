using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storym.Application.Diary;

using MediatR;
using Storym.Application.Abstactions;
using Storym.Domain.Diary;

public record CreateDiaryEntryCommand(
    string Title, 
    string Summary, 
    string Content, 
    DateTime Date, 
    bool IsHidden,
    IReadOnlyList<(string FileName, Stream Content)> Files): IRequest<DiaryEntryDto>;
public sealed class CreateDiaryEntryHandler : IRequestHandler<CreateDiaryEntryCommand, Storym.Application.Diary.DiaryEntryDto>
{
    private readonly IDiaryEntryRepository _repo;
    private readonly IFileStorage _files;
    private readonly ICurrentUser _user;
    private readonly IUserReadService _users;

    public CreateDiaryEntryHandler(IDiaryEntryRepository repo, IFileStorage files, ICurrentUser user, IUserReadService users)
    {
        _repo = repo; _files = files; _user = user;
        _users = users;
    }

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

        var owner = await _users.GetUserAsync(_user.UserId, ct)
                   ?? new UserMini(_user.UserId, "Unknown", "", null);

        return new Storym.Application.Diary.DiaryEntryDto(
            entry.Id, entry.Title, entry.Summary, entry.Content, entry.Date, entry.IsHidden, entry.UserId,
            entry.Images.Select(i => i.Path).ToList(), entry.Likes.Count, false, owner.Nick, owner.Email, owner.AvatarUrl,
            OwnerFollowing: !string.IsNullOrEmpty(_user.UserId));
    }
}