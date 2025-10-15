using Microsoft.EntityFrameworkCore;
using Storym.Application.Abstactions;
using Storym.Domain.Diary;

namespace Storym.Infrastructure.Persistence;

public sealed class DiaryEntryRepository : IDiaryEntryRepository
{
    private readonly AppDbContext _db;
    public DiaryEntryRepository(AppDbContext db) => _db = db;

    public Task<DiaryEntry?> GetAsync(int id, CancellationToken ct) =>
        _db.DiaryEntries.Include(x => x.Images).Include(x => x.Likes)
           .FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<List<DiaryEntry>> GetFeedAsync(string? viewerId, CancellationToken ct) =>
        _db.DiaryEntries.Where(d => !d.IsHidden)
           .Include(x => x.Images).Include(x => x.Likes)
           .OrderByDescending(d => d.Date)
           .ToListAsync(ct);

    public Task<List<DiaryEntry>> GetByUserAsync(string userId, bool includeHidden, CancellationToken ct) =>
        _db.DiaryEntries.Where(d => d.UserId == userId && (includeHidden || !d.IsHidden))
           .Include(x => x.Images).Include(x => x.Likes)
           .OrderByDescending(d => d.Date)
           .ToListAsync(ct);

    public async Task AddAsync(DiaryEntry entry, CancellationToken ct) => await _db.AddAsync(entry, ct);
    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
