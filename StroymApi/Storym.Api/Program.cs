using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Storym.Application;
using Storym.Application.Abstactions;
using Storym.Application.Diary;
using Storym.Application.Diary.Commands;
using Storym.Application.Diary.Queries;
using Storym.Application.Users.Commands;
using Storym.Application.Users.Queries;
using Storym.Infrastructure;
using Storym.Infrastructure.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtSettings>(jwtSection);
var jwt = jwtSection.Get<JwtSettings>() ?? new JwtSettings();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
        };
    });
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()
));

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseStaticFiles(); // to serve /uploads
app.UseAuthentication();
app.UseAuthorization();


// redirect to Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

// AUTH
app.MapPost("/auth/register",
    async ([FromBody] UserRegisterDto dto,
           [FromServices] UserManager<ApplicationUser> users) =>
    {
        var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email, UserNick = dto.Nick };
        var result = await users.CreateAsync(user, dto.Password);
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    });

app.MapPost("/auth/login",
    async ([FromBody] UserLoginDto dto,
           [FromServices] SignInManager<ApplicationUser> signIn,
           [FromServices] UserManager<ApplicationUser> users,
           [FromServices] IOptions<JwtSettings> opt) =>
    {
        var user = await users.FindByEmailAsync(dto.Email);
        if (user is null) return Results.Unauthorized();

        var ok = await signIn.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!ok.Succeeded) return Results.Unauthorized();

        var jwt = opt.Value;
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
        new Claim(ClaimTypes.NameIdentifier, user.Id),
    };
        var token = handler.CreateJwtSecurityToken(
            issuer: jwt.Issuer, audience: jwt.Audience,
            subject: new ClaimsIdentity(claims),
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return Results.Ok(new { token = handler.WriteToken(token) });
    });


var users = app.MapGroup("/users").RequireAuthorization();
users.MapGet("/{id}", async ([FromRoute] string id, [FromServices] ISender sender) =>
{
    var dto = await sender.Send(new GetUserProfileQuery(id));
    return Results.Ok(dto);
}).AllowAnonymous();

users.MapGet("/me", async ([FromServices] ISender sender) =>
{
    var dto = await sender.Send(new GetMyProfileQuery());
    return Results.Ok(dto);
});
users.MapPost("/me/avatar",
    async (HttpContext ctx,
           [FromServices] UserManager<ApplicationUser> userManager,
           [FromServices] IFileStorage storage,
           CancellationToken ct) =>
    {
        var userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? ctx.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        var form = await ctx.Request.ReadFormAsync(ct);
        var file = form.Files.FirstOrDefault();
        if (file is null || file.Length == 0) return Results.BadRequest("file is required");

        // Save file to /wwwroot/uploads and get the (relative) URL
        await using var stream = file.OpenReadStream();
        var newPath = await storage.SaveAsync(stream, file.FileName, ct);

        // Update user
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return Results.Unauthorized();

        // (Optional) delete old avatar if you want:
        // if (!string.IsNullOrWhiteSpace(user.ProfilePicturePath))
        //     await storage.DeleteAsync(user.ProfilePicturePath, ct);

        user.ProfilePicturePath = newPath;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded) return Results.BadRequest(result.Errors);

        return Results.Ok(new { avatarUrl = newPath });
    });

// GET /users/suggested?take=5
users.MapGet("/suggested",
    async ([FromQuery] int? take,
           HttpContext ctx,
           [FromServices] IUserReadService svc,
           CancellationToken ct) =>
    {
        var meId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? ctx.User.FindFirst("sub")?.Value;   // may be null (anonymous)
        var list = await svc.SearchAsync(query: null, take: take ?? 5, excludeUserId: meId, currentUserId: meId, ct);
        return Results.Ok(list);
    }).AllowAnonymous();

// GET /users/search?q=...&take=10
users.MapGet("/search",
    async ([FromQuery] string? q,
           [FromQuery] int? take,
           HttpContext ctx,
           [FromServices] IUserReadService svc,
           CancellationToken ct) =>
    {
        var meId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? ctx.User.FindFirst("sub")?.Value;
        var list = await svc.SearchAsync(query: q, take: take ?? 10, excludeUserId: meId, currentUserId: meId, ct);
        return Results.Ok(list);
    }).AllowAnonymous();

// POST /users/{id}/follow
users.MapPost("/{id}/follow",
    async ([FromRoute] string id, [FromServices] ISender sender) =>
    {
        var ok = await sender.Send(new FollowUserCommand(id));
        return Results.Ok(new { following = true, ok }); 
    }).RequireAuthorization();

// DELETE /users/{id}/follow
users.MapDelete("/{id}/follow",
    async ([FromRoute] string id, [FromServices] ISender sender) =>
    {
        var ok = await sender.Send(new UnfollowUserCommand(id));
        return Results.Ok(new { following = false, ok });
    }).RequireAuthorization();

// GET /users/{id}/follow-state
users.MapGet("/{id}/follow-state",
    async ([FromRoute] string id, [FromServices] ISender sender) =>
    {
        var f = await sender.Send(new GetFollowStateQuery(id));
        return Results.Ok(new { following = f });
    }).AllowAnonymous();

// GET /users/{id}/followers?page=1&pageSize=20
users.MapGet("/{id}/followers",
    async ([FromRoute] string id, [FromQuery] int page, [FromQuery] int pageSize, [FromServices] ISender sender) =>
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);
        var dto = await sender.Send(new GetFollowersPageQuery(id, page, pageSize));
        return Results.Ok(dto);
    }).AllowAnonymous();

// GET /users/{id}/following?page=1&pageSize=20
users.MapGet("/{id}/following",
    async ([FromRoute] string id, [FromQuery] int page, [FromQuery] int pageSize, [FromServices] ISender sender) =>
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 20 : (pageSize > 100 ? 100 : pageSize); 
        var dto = await sender.Send(new GetFollowingPageQuery(id, page, pageSize));
        return Results.Ok(dto);
    }).AllowAnonymous();


// DIARY
var diary = app.MapGroup("/api/diary").RequireAuthorization();

diary.MapPost("/",
    async ([FromServices] ISender sender,
           HttpRequest http) =>
    {
        var form = await http.ReadFormAsync();
        var files = form.Files.Select(f => (f.FileName, (Stream)f.OpenReadStream())).ToList();

        var dto = await sender.Send(new CreateDiaryEntryCommand(
            Title: form["Title"]!,
            Summary: form["Summary"]!,
            Content: form["Content"]!,
            Date: DateTime.Parse(form["Date"]!),
            IsHidden: bool.Parse(form["IsHidden"]!),
            Files: files
        ));

        return Results.Created($"/api/diary/{dto.Id}", dto);
    });

diary.MapPut("/{id:int}",
    async ([FromRoute] int id,
           [FromServices] ISender sender,
           HttpRequest http) =>
    {
        var form = await http.ReadFormAsync();

        var deleteImages = form["DeleteImages"]
            .ToString()
            .Split(';', StringSplitOptions.RemoveEmptyEntries);

        var files = form.Files.Select(f => (f.FileName, (Stream)f.OpenReadStream())).ToList();

        var dto = await sender.Send(new EditDiaryEntryCommand(
            Id: id,
            Title: form["Title"]!,
            Summary: form["Summary"]!,
            Content: form["Content"]!,
            Date: DateTime.Parse(form["Date"]!),
            IsHidden: bool.Parse(form["IsHidden"]!),
            DeleteImages: deleteImages,
            NewFiles: files
        ));

        return Results.Ok(dto);
    });

diary.MapPost("/{id:int}/like",
    async ([FromRoute] int id,
           [FromServices] ISender sender) =>
    {
        var count = await sender.Send(new ToggleLikeCommand(id));
        return Results.Ok(new { likes = count });
    });

diary.MapGet("/feed",
    async ([FromServices] ISender sender) =>
        Results.Ok(await sender.Send(new GetFeedQuery()))
);

diary.MapGet("/by-user/{id}", async ([FromRoute] string id, [FromServices] ISender sender) =>
{
    var list = await sender.Send(new GetUserPostsQuery(id));
    return Results.Ok(list);
}).AllowAnonymous();

diary.MapGet("/me",
    async ([FromServices] ISender sender) =>
        Results.Ok(await sender.Send(new GetMineQuery()))
);

diary.MapGet("/user/{uid}",
    async ([FromRoute] string uid,
           [FromServices] ISender sender) =>
        Results.Ok(await sender.Send(new GetByUserQuery(uid)))
);

diary.MapGet("/{id:int}",
    async ([FromRoute] int id,
           [FromServices] ISender sender) =>
        Results.Ok(await sender.Send(new GetDetailsQuery(id)))
);

app.Run();

public sealed record UserRegisterDto(string Email, string Nick, string Password);
public sealed record UserLoginDto(string Email, string Password);

public sealed class JwtSettings
{
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public string Key { get; set; } = "";
}
