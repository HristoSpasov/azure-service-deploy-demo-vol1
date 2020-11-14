namespace MessageSender
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using omg_app;
    using omg_app.Models;

    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = GetConfiguration();
            var serviceProvider = GetServices(configuration);

            EmitMessage(configuration).GetAwaiter().GetResult();
        }

        private static async Task EmitMessage(IConfiguration configuration)
        {
            var serviceBusConfiguration = GetServiceBusConfiguration(configuration);
            ServiceBusService.SetupServiceBus(serviceBusConfiguration, false);
            await ServiceBusService.AddMessageAsync(Console.ReadLine());
        }

        private static ServiceBusModel GetServiceBusConfiguration(IConfiguration configuration)
        {
            var busConfiguration = configuration.GetSection("AzureServiceBus").Get<ServiceBusModel>();

            if (busConfiguration == null)
            {
                throw new ArgumentException("Invalid azure service bus configuration.");
            }

            var hasInvalidConfiguration = busConfiguration.GetType().GetProperties().Select(prop => prop.GetValue(busConfiguration).ToString()).Any(string.IsNullOrWhiteSpace);

            if (hasInvalidConfiguration)
            {
                throw new ArgumentException("Invalid azure service bus configuration properties.");
            }

            return busConfiguration;
        }

        private static IConfiguration GetConfiguration()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        private static IServiceProvider GetServices(IConfiguration configuration)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddLogging();
            services.AddOptions();
            services.AddSingleton(provider => configuration);

            return services.BuildServiceProvider();
        }
    }
}