using System;
using System.Threading.Tasks;
using Lightpack;
using Microsoft.Extensions.Options;
using prismatik2mqtt.Client;
using prismatik2mqtt.Configuration;
using Quartz;

namespace prismatik2mqtt.Jobs
{
    public class HealthCheckJob : IJob
    {
        private readonly LightpackMqttClient _lightpackMqttClient;
        private readonly MqttConfiguration _mqttConfig;
        private readonly LightpackConfiguration _lightpackConfig;

        public HealthCheckJob(LightpackMqttClient lightpackMqttClient, IOptions<MqttConfiguration> mqttOptions, IOptions<LightpackConfiguration> lightpackOptions)
        {
            _lightpackMqttClient = lightpackMqttClient;
            _mqttConfig = mqttOptions.Value;
            _lightpackConfig = lightpackOptions.Value;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var isMqttConnected = _lightpackMqttClient.Connect();
            if (!isMqttConnected)
            {
                await Console.Out.WriteLineAsync($"ERROR: Could not connect to MQTT server at {_mqttConfig.Host}.");
                return;
            }

            var status = _lightpackMqttClient.PublishStatus();
            if(status == Status.Error)
                await Console.Out.WriteLineAsync($"ERROR: Could not connect to lightpack at {_lightpackConfig.Host}:{_lightpackConfig.Port}.");
        }
    }
}