using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storym.Application.Abstactions;

using Storym.Domain.Diary;
public interface IDiaryEntryRepository
{
    Task<DiaryEntry?> GetAsync(int id, CancellationToken ct);
    Task<List<DiaryEntry>> GetFeedAsync(string? viewerId, CancellationToken ct); // public feed
    Task<List<DiaryEntry>> GetByUserAsync(string userId, bool includeHidden, CancellationToken ct);
    Task AddAsync(DiaryEntry entry, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}