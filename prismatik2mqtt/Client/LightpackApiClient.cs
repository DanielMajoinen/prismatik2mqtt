using Lightpack;
using Microsoft.Extensions.Options;
using prismatik2mqtt.Configuration;

namespace prismatik2mqtt.Client
{
    public class LightpackApiClient
    {
        private readonly LightpackConfiguration _configuration;
        
        public LightpackApiClient(IOptions<LightpackConfiguration> configuration)
        {
            _configuration = configuration.Value;
        }

        public Status GetStatus()
        {
            using var lightpack = new ApiLightpack(_configuration.Host, _configuration.Port);

            return lightpack.Connect()
                ? lightpack.GetStatus()
                : Status.Error;
        }

        public bool SetStatus(Status status)
        {
            using var lightpack = new ApiLightpack(_configuration.Host, _configuration.Port);

            if (!lightpack.Connect())
                return false;

            lightpack.SetStatus(status);

            return true;
        }
    }
}