using Application.Notifications.Commands.CreateNotification;
using Infrastructure.DependencyInjection;
using System.Text.Json.Serialization;
using FluentValidation.AspNetCore;
using API.Validators;
using FluentValidation;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: true));
            });

            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssemblyContaining<CreateNotificationRequestValidator>();

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.UseInlineDefinitionsForEnums();
            });

            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddScoped<CreateNotificationCommandHandler>();


            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.MapControllers();

            app.Run();
        }
    }
}

public partial class Program { }
