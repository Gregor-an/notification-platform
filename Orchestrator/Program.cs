using Infrastructure.DependencyInjection;
using Orchestrator.Workers;

namespace Orchestrator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddHostedService<NotificationProcessingWorker>();

            var host = builder.Build();
            host.Run();
        }
    }
}