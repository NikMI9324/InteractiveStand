
using InteractiveStand.Application.Interfaces;
using InteractiveStand.Domain.Classes;
using MQTTnet;
using System.Text;
using System.Text.Json;

namespace InteractiveStand.Application.Services.Mqtt
{
    public class MqttSimulationPublisherService : IMqttSimulationPublisher
    {
        private readonly IMqttClient _mqttClient;
        private readonly MqttClientOptions _mqttOptions;
        public MqttSimulationPublisherService()
        {
            _mqttClient = new MqttClientFactory().CreateMqttClient();
            _mqttOptions = new MqttClientOptionsBuilder()
                                .WithTcpServer("192.168.55.183", 1883)
                                .WithCredentials("hello", "hello")
                                .Build();
        }
        public async Task PublishRegionConsumerStatusAsync(ConsumerBinding consumerBinding, double currentTime, CancellationToken cancellationToken)
        {
            if (!_mqttClient.IsConnected)
                await _mqttClient.ConnectAsync(_mqttOptions, cancellationToken);
            if (string.IsNullOrWhiteSpace(consumerBinding.MacAddress))
                return;

            var message = new
            {
                status = consumerBinding.Region?.IsActive ?? false,
                time = currentTime
            };
            var payload = JsonSerializer.Serialize(message);

            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic($"consumer/{consumerBinding.MacAddress}")
                .WithPayload(Encoding.UTF8.GetBytes(payload))
                .Build();

            await _mqttClient.PublishAsync(mqttMessage, cancellationToken);
        }
    }
}
