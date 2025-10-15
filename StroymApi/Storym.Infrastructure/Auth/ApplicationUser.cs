using Microsoft.AspNetCore.Identity;

namespace Storym.Infrastructure.Auth;

public sealed class ApplicationUser : IdentityUser
{
    public string UserNick { get; set; } = string.Empty;
    public string? ProfilePicturePath { get; set; }
    public string? ProfileBannerPath { get; set; }
    public string AboutMe { get; set; } = string.Empty;
}
