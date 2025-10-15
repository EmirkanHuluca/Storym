namespace Storym.Domain.Diary;

public sealed class DiaryEntry
{
    private DiaryEntry() { }

    public DiaryEntry(string userId, string title, string summary, string content, DateTime date, bool isHidden)
    {
        UserId = userId; Title = title; Summary = summary; Content = content; Date = date; IsHidden = isHidden;
    }

    public int Id { get; private set; }
    public string UserId { get; private set; } = default!;
    public string Title { get; private set; } = default!;
    public string Summary { get; private set; } = default!;
    public string Content { get; private set; } = default!;
    public DateTime Date { get; private set; }
    public bool IsHidden { get; private set; }

    private readonly List<DiaryImage> _images = new();
    public IReadOnlyCollection<DiaryImage> Images => _images;

    private readonly List<Like> _likes = new();
    public IReadOnlyCollection<Like> Likes => _likes;

    public void Edit(string title, string summary, string content, DateTime date, bool isHidden)
    { Title = title; Summary = summary; Content = content; Date = date; IsHidden = isHidden; }

    public void ToggleLike(string userId)
    {
        var existing = _likes.FirstOrDefault(l => l.UserId == userId);
        if (existing is null) _likes.Add(new Like(Id, userId));
        else _likes.Remove(existing);
    }

    public void AddImage(string path) => _images.Add(new DiaryImage(Id, path));
    public void RemoveImage(string path) => _images.RemoveAll(i => i.Path == path);
}

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

    /* Id, DiaryEntryId, Path */
}
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

    /* Id, DiaryEntryId, UserId */
}
