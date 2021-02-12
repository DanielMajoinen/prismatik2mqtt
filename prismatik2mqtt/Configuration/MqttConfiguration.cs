namespace prismatik2mqtt.Configuration
{
    public class MqttConfiguration
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public TopicsConfiguration Topics { get; set; }
        public int QoS { get; set; }
    }
}