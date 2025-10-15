namespace Storym.Domain.Diary;

public sealed class Like
{
    private Like() { }
    public Like(int diaryEntryId, string userId)
    {
        DiaryEntryId = diaryEntryId;
        UserId = userId;
    }
    public int Id { get; private set; }
    public int DiaryEntryId { get; private set; }
    public string UserId { get; private set; } = default!;
}
