using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Storym.Application.Abstactions;
using Storym.Application.Abstractions;
using Storym.Infrastructure.Auth;
using Storym.Infrastructure.Persistence;
using Storym.Infrastructure.Repositories;
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
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddScoped<IFileStorage, LocalFileStorage>();
        services.AddScoped<IDiaryEntryRepository, DiaryEntryRepository>();
        services.AddScoped<IClock, SystemClock>();
        services.AddScoped<IUserReadService, UserReadService>();
        services.AddScoped<IFollowRepository, FollowRepository>();
        services.AddScoped<IFollowReadService, FollowReadService>();

        return services;
    }
}
