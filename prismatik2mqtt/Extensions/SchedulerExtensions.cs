using System.Threading.Tasks;
using Quartz;

namespace prismatik2mqtt.Extensions
{
    public static class SchedulerExtensions
    {
        public static async Task Schedule<T>(this IScheduler scheduler, string jobName, string triggerName, string groupName, int interval) where T : IJob
        {
            var job = JobBuilder.Create<T>()
                .WithIdentity(jobName, groupName)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity(triggerName, groupName)
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(interval)
                    .RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }
}