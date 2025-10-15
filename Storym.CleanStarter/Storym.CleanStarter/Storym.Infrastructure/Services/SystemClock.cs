using Storym.Application.Abstractions;

namespace Storym.Infrastructure.Services;
public sealed class SystemClock : IClock { public DateTime UtcNow => DateTime.UtcNow; }
