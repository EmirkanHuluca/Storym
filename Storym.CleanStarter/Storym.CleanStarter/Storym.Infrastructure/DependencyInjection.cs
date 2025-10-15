using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Storym.Application.Abstractions;
using Storym.Infrastructure.Auth;
using Storym.Infrastructure.Persistence;
using Storym.Infrastructure.Services;

namespace Storym.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(o =>
            o.UseSqlServer(config.GetConnectionString("Default")));

        services.AddIdentityCore<ApplicationUser>(options => { })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddScoped<IFileStorage, LocalFileStorage>();
        services.AddScoped<IDiaryEntryRepository, DiaryEntryRepository>();
        services.AddScoped<IClock, SystemClock>();

        return services;
    }
}
