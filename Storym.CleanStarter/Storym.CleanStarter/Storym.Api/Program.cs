using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Storym.Application;
using Storym.Application.Diary;
using Storym.Application.Diary.Commands;
using Storym.Application.Diary.Queries;
using Storym.Infrastructure;
using Storym.Infrastructure.Auth;

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

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles(); // to serve /uploads
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/swagger"));

// Auth: register
app.MapPost("/auth/register", async (UserRegisterDto dto, UserManager<ApplicationUser> users) =>
{
    var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email, UserNick = dto.Nick };
    var result = await users.CreateAsync(user, dto.Password);
    return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
});

// Auth: login
app.MapPost("/auth/login", async (UserLoginDto dto, SignInManager<ApplicationUser> signIn,
                                  UserManager<ApplicationUser> users, IOptions<JwtSettings> opt) =>
{
    var user = await users.FindByEmailAsync(dto.Email);
    if (user is null) return Results.Unauthorized();
    var check = await signIn.CheckPasswordSignInAsync(user, dto.Password, false);
    if (!check.Succeeded) return Results.Unauthorized();

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
        issuer: jwt.Issuer,
        audience: jwt.Audience,
        subject: new ClaimsIdentity(claims),
        expires: DateTime.UtcNow.AddDays(7),
        signingCredentials: creds);
    var serialized = handler.WriteToken(token);
    return Results.Ok(new { token = serialized });
});

var diary = app.MapGroup("/api/diary").RequireAuthorization();

// POST /api/diary  (multipart/form-data)
diary.MapPost("/", async (HttpRequest http, ISender sender) =>
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

// PUT /api/diary/{id}
diary.MapPut("/{id:int}", async (int id, HttpRequest http, ISender sender) =>
{
    var form = await http.ReadFormAsync();
    var deleteImages = form["DeleteImages"].ToString().Split(';', StringSplitOptions.RemoveEmptyEntries);
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

// POST /api/diary/{id}/like
diary.MapPost("/{id:int}/like", async (int id, ISender sender) =>
{
    var count = await sender.Send(new ToggleLikeCommand(id));
    return Results.Ok(new { likes = count });
});

// GET /api/diary/feed
diary.MapGet("/feed", async (ISender sender) =>
{
    var list = await sender.Send(new GetFeedQuery());
    return Results.Ok(list);
});

// GET /api/diary/me
diary.MapGet("/me", async (ISender sender) =>
{
    var list = await sender.Send(new GetMineQuery());
    return Results.Ok(list);
});

// GET /api/diary/user/{uid}
diary.MapGet("/user/{uid}", async (string uid, ISender sender) =>
{
    var list = await sender.Send(new GetByUserQuery(uid));
    return Results.Ok(list);
});

// GET /api/diary/{id}
diary.MapGet("/{id:int}", async (int id, ISender sender) =>
{
    var dto = await sender.Send(new GetDetailsQuery(id));
    return Results.Ok(dto);
});

app.Run();

public sealed record UserRegisterDto(string Email, string Nick, string Password);
public sealed record UserLoginDto(string Email, string Password);

public sealed class JwtSettings
{
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public string Key { get; set; } = "";
}
