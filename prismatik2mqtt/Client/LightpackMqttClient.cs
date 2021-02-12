using System;
using System.Text;
using Lightpack;
using M2Mqtt;
using M2Mqtt.Messages;
using Microsoft.Extensions.Options;
using prismatik2mqtt.Configuration;

namespace prismatik2mqtt.Client
{
    public class LightpackMqttClient
    {
        public static string BaseTopic = "prismatik2mqtt";
        public static string DeviceTopcic = "daniel-monitor";
        public static string CommandTopic = "cmd";
        public static string StateTopic = "state";
        public static byte QosLevel = MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE;

        public bool IsConnected => _mqttClient.IsConnected;

        private readonly LightpackApiClient _lightpackApiClient;
        private readonly MqttConfiguration _mqttConfig;
        private readonly MqttClient _mqttClient;

        public LightpackMqttClient(LightpackApiClient lightpackApiClient, IOptions<MqttConfiguration> mqttConfig)
        {
            _lightpackApiClient = lightpackApiClient;
            _mqttConfig = mqttConfig.Value;
            _mqttClient = new MqttClient(mqttConfig.Value.Host);
        }

        public bool Connect()
        {
            if (IsConnected)
                return IsConnected;

            var clientId = Guid.NewGuid().ToString();

            _mqttClient.Connect(clientId, _mqttConfig.Username, _mqttConfig.Password);

            if (IsConnected)
            {
                Observe();
                Subscribe();
                PublishStatus();
            }

            return IsConnected;
        }

        public Status PublishStatus()
        {
            // Get current state and publish
            var status = _lightpackApiClient.GetStatus();
            var state = status != Status.Error
                ? status.ToString().ToUpper()
                : "OFF";

            _mqttClient.Publish($"{BaseTopic}/{DeviceTopcic}/{StateTopic}", Encoding.UTF8.GetBytes(state), QosLevel, false);

            return status;
        }

        private void Observe()
        {
            // Observe published messages
            _mqttClient.MqttMsgPublishReceived += (sender, args) => PublishedMsgReceived(sender, args, _lightpackApiClient);
        }

        private void Subscribe()
        {
            // Subscribe to topic
            _mqttClient.Subscribe(new[] { $"{BaseTopic}/{DeviceTopcic}/{CommandTopic}" }, new[] { QosLevel });
        }

        protected static void PublishedMsgReceived(object sender, MqttMsgPublishEventArgs e, LightpackApiClient lightpackApiClient)
        {
            var msg = Encoding.UTF8.GetString(e.Message).ToUpper();
            var success = msg switch
            {
                "ON" => lightpackApiClient.SetStatus(Status.On),
                "OFF" => lightpackApiClient.SetStatus(Status.Off),
                _ => false
            };

            // No need to publish state if we couldn't change it
            if (!success)
                return;

            var mqttClient = (MqttClient) sender;
            mqttClient.Publish($"{BaseTopic}/{DeviceTopcic}/{StateTopic}", Encoding.UTF8.GetBytes(msg), QosLevel, false);
        }
    }
}