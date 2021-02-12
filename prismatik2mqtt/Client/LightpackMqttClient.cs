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
        private byte QosLevel => (byte) _mqttConfig.QoS;
        private string StateTopic => $"{_mqttConfig.Topics.Base}/{_mqttConfig.Topics.Device}/{_mqttConfig.Topics.State}";
        private string CommandTopic => $"{_mqttConfig.Topics.Base}/{_mqttConfig.Topics.Device}/{_mqttConfig.Topics.Command}";
        
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
            if (_mqttClient.IsConnected)
                return _mqttClient.IsConnected;

            var clientId = Guid.NewGuid().ToString();

            _mqttClient.Connect(clientId, _mqttConfig.Username, _mqttConfig.Password);

            if (_mqttClient.IsConnected)
            {
                Observe();
                Subscribe();
                PublishStatus();
            }

            return _mqttClient.IsConnected;
        }

        public Status PublishStatus()
        {
            // Get current state and publish
            var status = _lightpackApiClient.GetStatus();
            var state = status != Status.Error
                ? status.ToString().ToUpper()
                : "OFF";

            _mqttClient.Publish(StateTopic, Encoding.UTF8.GetBytes(state), QosLevel, false);

            return status;
        }

        private void Observe()
        {
            // Observe published messages
            _mqttClient.MqttMsgPublishReceived += (sender, args) 
                => PublishedMsgReceived(sender, args, _lightpackApiClient, StateTopic, QosLevel);
        }

        private void Subscribe()
        {
            // Subscribe to topic
            _mqttClient.Subscribe(new[] { CommandTopic }, new[] { QosLevel });
        }

        protected static void PublishedMsgReceived(object sender, MqttMsgPublishEventArgs e, LightpackApiClient lightpackApiClient, string stateTopic, byte qosLevel)
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

            mqttClient.Publish(stateTopic, Encoding.UTF8.GetBytes(msg), qosLevel, false);
        }
    }
}