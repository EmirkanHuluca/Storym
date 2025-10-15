namespace Storym.Application.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}
