using Storym.Application.Abstactions;

namespace Storym.Infrastructure.Services;
public sealed class SystemClock : IClock { public DateTime UtcNow => DateTime.UtcNow; }
