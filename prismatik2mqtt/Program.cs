using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace prismatik2mqtt
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Create service collection
            var serviceCollection = new ServiceCollection();
            serviceCollection.ConfigureServices();
            
            // Build service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Get the job scheduler
            var jobSchedulerFactory = serviceProvider.GetService<ISchedulerFactory>();
            if (jobSchedulerFactory == null)
            {
                Console.WriteLine("ERROR: Unable to initiate jobs.");
                return;
            }

            var jobScheduler = await jobSchedulerFactory.GetScheduler();
            
            // Start jobs
            await jobScheduler.Start();

            Console.WriteLine("Press any key to close the application");
            Console.Read();

            // Shutdown jobs before exiting
            await jobScheduler.Shutdown(true);
        }
    }
}
