
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
        private bool _triedToConnect = false;
        private bool _isConnectionFailed = false;
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
            
            if (string.IsNullOrWhiteSpace(consumerBinding.MacAddress))
                return;
            if (!_mqttClient.IsConnected && !_isConnectionFailed)
            {
                try
                {
                    if (!_triedToConnect)
                    {
                        _triedToConnect = true;
                        await _mqttClient.ConnectAsync(_mqttOptions, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _isConnectionFailed = true;
                    Console.WriteLine($"[MQTT] Ошибка подключения: {ex.Message}");
                    return; 
                }
            }
            if (!_mqttClient.IsConnected)
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
