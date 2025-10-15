namespace Storym.Domain.Diary;

public sealed class DiaryImage
{
    private DiaryImage() { }
    public DiaryImage(int diaryEntryId, string path)
    {
        DiaryEntryId = diaryEntryId;
        Path = path;
    }
    public int Id { get; private set; }
    public int DiaryEntryId { get; private set; }
    public string Path { get; private set; } = default!;
}
