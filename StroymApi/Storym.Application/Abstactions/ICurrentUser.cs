using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storym.Application.Abstactions;

public interface ICurrentUser { string UserId { get; } bool IsInRole(string role); }

public interface IClock { DateTime UtcNow { get; } }

public interface IFileStorage { Task<string> SaveAsync(Stream content, string fileName, CancellationToken ct); Task DeleteAsync(string path, CancellationToken ct); }
