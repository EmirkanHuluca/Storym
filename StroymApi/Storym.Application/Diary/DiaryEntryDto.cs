using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    bool LikedByMe,
    string OwnerNick,          
    string OwnerEmail,         
    string? OwnerAvatarUrl,
    bool OwnerFollowing

);