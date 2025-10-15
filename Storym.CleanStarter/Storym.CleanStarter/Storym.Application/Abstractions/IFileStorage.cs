namespace Storym.Application.Abstractions;

public interface IFileStorage
{
    Task<string> SaveAsync(System.IO.Stream content, string fileName, System.Threading.CancellationToken ct);
    Task DeleteAsync(string path, System.Threading.CancellationToken ct);
}
