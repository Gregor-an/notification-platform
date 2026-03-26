using Application.Interfaces.Providers;
using Application.Interfaces.Repositories;
using Infrastructure.Persistence;
using Infrastructure.Providers;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("Default")));

            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<INotificationSender, MockEmailNotificationSender>();

            return services;
        }
    }
}
