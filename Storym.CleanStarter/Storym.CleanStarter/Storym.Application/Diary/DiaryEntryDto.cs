namespace Storym.Application.Diary;

public record DiaryEntryDto(
    int Id,
    string Title,
    string Summary,
    string Content,
    DateTime Date,
    bool IsHidden,
    string UserId,
    IReadOnlyList<string> ImageUrls,
    int LikeCount,
    bool LikedByMe);
