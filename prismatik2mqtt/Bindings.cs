using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using prismatik2mqtt.Client;
using prismatik2mqtt.Configuration;
using prismatik2mqtt.Jobs;
using Quartz;

namespace prismatik2mqtt
{
    public static class Bindings
    {
        public static void ConfigureServices(this IServiceCollection serviceCollection)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            serviceCollection.AddLogging();
            serviceCollection.AddOptions();

            var lightpackSection = configuration.GetSection("Lightpack");
            serviceCollection.Configure<LightpackConfiguration>(lightpackSection);

            var mqttSection = configuration.GetSection("Mqtt");
            serviceCollection.Configure<MqttConfiguration>(mqttSection);
            
            serviceCollection.AddTransient<LightpackApiClient>();
            serviceCollection.AddTransient<LightpackMqttClient>();
            serviceCollection.AddTransient<HealthCheckJob>();

            var healthCheckJobKey = new JobKey("healthCheckJob");
            serviceCollection.AddQuartz(q =>
            {
                q.SchedulerId = "JobScheduler";
                q.SchedulerName = "Job Scheduler";
                q.UseMicrosoftDependencyInjectionJobFactory();
                q.AddJob<HealthCheckJob>(j => j.WithIdentity(healthCheckJobKey));
                q.AddTrigger(t => t
                    .WithIdentity("healthCheckJobTrigger")
                    .ForJob(healthCheckJobKey)
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(10)
                        .RepeatForever()));
            });
        }
    }
}