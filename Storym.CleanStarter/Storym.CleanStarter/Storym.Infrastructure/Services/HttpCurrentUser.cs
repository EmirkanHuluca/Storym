using Storym.Application.Abstractions;

namespace Storym.Infrastructure.Services;

public sealed class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _ctx;
    public HttpCurrentUser(IHttpContextAccessor ctx) => _ctx = ctx;
    public string UserId =>
        _ctx.HttpContext?.User.FindFirst("sub")?.Value
        ?? _ctx.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? throw new UnauthorizedAccessException();

    public bool IsInRole(string role) => _ctx.HttpContext?.User.IsInRole(role) ?? false;
}
