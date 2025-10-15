using Storym.Domain.Diary;

namespace Storym.Application.Abstractions;

public interface IDiaryEntryRepository
{
    Task<DiaryEntry?> GetAsync(int id, CancellationToken ct);
    Task<List<DiaryEntry>> GetFeedAsync(string? viewerId, CancellationToken ct);
    Task<List<DiaryEntry>> GetByUserAsync(string userId, bool includeHidden, CancellationToken ct);
    Task AddAsync(DiaryEntry entry, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
