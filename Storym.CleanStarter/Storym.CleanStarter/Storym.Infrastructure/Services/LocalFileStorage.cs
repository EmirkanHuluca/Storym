using Storym.Application.Abstractions;

namespace Storym.Infrastructure.Services;

public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _root;
    public LocalFileStorage(IWebHostEnvironment env)
    {
        _root = Path.Combine(env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAsync(Stream content, string fileName, CancellationToken ct)
    {
        var name = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var path = Path.Combine(_root, name);
        using var fs = new FileStream(path, FileMode.Create);
        await content.CopyToAsync(fs, ct);
        return $"/uploads/{name}";
    }

    public Task DeleteAsync(string path, CancellationToken ct)
    {
        var full = Path.Combine(_root, Path.GetFileName(path));
        if (File.Exists(full)) File.Delete(full);
        return Task.CompletedTask;
    }
}
