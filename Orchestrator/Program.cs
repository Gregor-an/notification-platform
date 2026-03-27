using Infrastructure.DependencyInjection;
using Orchestrator.Services;
using Orchestrator.Settings;
using Orchestrator.Workers;

namespace Orchestrator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddInfrastructure(builder.Configuration);
            
            builder.Services.Configure<NotificationProcessingOptions>(
                builder.Configuration.GetSection(NotificationProcessingOptions.SectionName));

            builder.Services.AddScoped<NotificationProcessingService>();
            builder.Services.AddHostedService<NotificationProcessingWorker>();

            var host = builder.Build();
            host.Run();
        }
    }
}