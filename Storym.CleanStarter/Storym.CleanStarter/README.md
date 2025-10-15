# Storym Clean Architecture Starter (ASP.NET Core + React API backend)

This is a **from-scratch** Clean Architecture starter for your diary app.
It contains 4 projects: **Storym.Domain**, **Storym.Application**, **Storym.Infrastructure**, **Storym.Api**.

> Open **Package Manager Console** and run the NuGet commands below **from the solution root** (you can keep the Default project dropdown anywhere because we pass `-Project` explicitly).

## 1) Create a new Solution in Visual Studio
- Create an empty solution named `Storym` in a parent folder.
- In Solution Explorer: **Add → Existing Project...** for each of the four `.csproj` files in this folder.

Projects:
```
Storym.Domain/Storym.Domain.csproj
Storym.Application/Storym.Application.csproj
Storym.Infrastructure/Storym.Infrastructure.csproj
Storym.Api/Storym.Api.csproj
```

Then set **Storym.Api** as the Startup Project.

## 2) Install NuGet packages (PMC)

```powershell
# Application
Install-Package MediatR.Extensions.Microsoft.DependencyInjection -Project Storym.Application
Install-Package FluentValidation -Project Storym.Application

# Infrastructure
Install-Package Microsoft.EntityFrameworkCore -Project Storym.Infrastructure
Install-Package Microsoft.EntityFrameworkCore.SqlServer -Project Storym.Infrastructure
Install-Package Microsoft.EntityFrameworkCore.Design -Project Storym.Infrastructure
Install-Package Microsoft.AspNetCore.Identity.EntityFrameworkCore -Project Storym.Infrastructure
Install-Package Microsoft.Extensions.Identity.Core -Project Storym.Infrastructure
Install-Package Microsoft.EntityFrameworkCore.Tools -Project Storym.Infrastructure

# API
Install-Package Microsoft.AspNetCore.Authentication.JwtBearer -Project Storym.Api
Install-Package Swashbuckle.AspNetCore -Project Storym.Api
```

## 3) Set connection string
Edit **Storym.Api/appsettings.json**:
```json
{
  "ConnectionStrings": {
    "Default": "Server=.;Database=Storym;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Issuer": "Storym.Api",
    "Audience": "Storym.Web",
    "Key": "CHANGE_ME_MIN_32_CHARS_LONG_SECRET"
  },
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*"
}
```

## 4) Create DB
PMC:
```powershell
Add-Migration Initial -Project Storym.Infrastructure -StartupProject Storym.Api
Update-Database -Project Storym.Infrastructure -StartupProject Storym.Api
```

## 5) Run
- Start **Storym.Api**
- Swagger should open; you can register/login and call `/api/diary` endpoints.
- You’ll build your React front end separately and call these endpoints.

---

### Folder map
```
Storym.Domain
  └── Diary/DiaryEntry.cs, DiaryImage.cs, Like.cs

Storym.Application
  ├── Abstractions/ICurrentUser.cs, IClock.cs, IFileStorage.cs, IDiaryEntryRepository.cs
  ├── Diary/DiaryEntryDto.cs
  ├── Diary/Commands (Create, Edit, ToggleLike)
  ├── Diary/Queries  (GetFeed, GetMine, GetByUser, GetDetails)
  ├── DependencyInjection.cs
  └── Common/Exceptions/ForbiddenAccessException.cs

Storym.Infrastructure
  ├── Auth/ApplicationUser.cs
  ├── Persistence/AppDbContext.cs, DiaryEntryRepository.cs
  ├── Services/LocalFileStorage.cs, HttpCurrentUser.cs, SystemClock.cs
  └── DependencyInjection.cs

Storym.Api
  ├── Program.cs
  └── appsettings.json
```

Happy building! 🚀
