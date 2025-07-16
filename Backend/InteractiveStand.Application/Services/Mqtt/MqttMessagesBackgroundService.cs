using InteractiveStand.Application.EspMessages;
using InteractiveStand.Application.Interfaces;
using InteractiveStand.Domain.Classes;
using InteractiveStand.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MQTTnet;
using MQTTnet.Packets;
using System.Text;
using System.Text.Json;

namespace InteractiveStand.Application.Services.Mqtt
{
    public class MqttMessagesBackgroundService : BackgroundService, IMqttBackgroundPublisher
    {
        private readonly IMqttClient _mqttClient;
        private readonly MqttClientOptions _mqttOptions;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public MqttMessagesBackgroundService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;

            var factory = new MqttClientFactory();
            _mqttClient = factory.CreateMqttClient();

            _mqttOptions = new MqttClientOptionsBuilder()
                                .WithTcpServer("localhost", 1883)
                                .WithCredentials("IES", "practice")
                                .Build();
            _mqttClient.ApplicationMessageReceivedAsync += HandleMessageAsync;
            
            
        }

        private async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs args)
        {
            var topic = args.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
            using var scope = _serviceScopeFactory.CreateScope();
            if (topic == "esp/update")
            {
                var message = JsonSerializer.Deserialize<EspUpdateMessage>(payload);
                if (message == null) return;
                var updateHandler = scope.ServiceProvider.GetRequiredService<IUpdateMessageHandler>();
                await updateHandler.HandleAsync(message, CancellationToken.None);
            }
            else if (topic == "esp/connect")
            {
                var message = JsonSerializer.Deserialize<EspConnectMessage>(payload);
                if(message == null) return;
                var connectHandler = scope.ServiceProvider.GetRequiredService<IConnectMessageHandler>();
                await connectHandler.HandleAsync(message, CancellationToken.None);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _mqttClient.ConnectAsync(_mqttOptions, stoppingToken);

                var topicFilters = new List<MqttTopicFilter>
                {
                    new MqttTopicFilterBuilder().WithTopic("esp/update").Build(),
                    new MqttTopicFilterBuilder().WithTopic("esp/connect").Build()
                };

                await _mqttClient.SubscribeAsync(new MqttClientSubscribeOptions
                {
                    TopicFilters = topicFilters
                }, stoppingToken);
                while(!stoppingToken.IsCancellationRequested)
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var regionRepo = scope.ServiceProvider.GetRequiredService<IRegionRepository>();
                    var producerBindings = await regionRepo.GetProducerBindingsWithRegionAsync(stoppingToken);

                    foreach (var producerBinding in producerBindings)
                    {
                        await PublishProducedCapacityAsync(producerBinding, producerBinding.Region.PowerSource, stoppingToken);
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(500), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MQTT connection error: {ex.Message}");
            }
        }

        public async Task PublishProducedCapacityAsync(ProducerBinding producerBinding, PowerSource powerSource, CancellationToken cancellationToken)
        {
            if (!_mqttClient.IsConnected)
                await _mqttClient.ConnectAsync(_mqttOptions, cancellationToken);

            if (string.IsNullOrWhiteSpace(producerBinding.MacAddress))
                return;

            var currentCapacity = powerSource.GetProducerCapacity(producerBinding.CapacityProducerType);

            var message = new
            {
                power = currentCapacity
            };

            var payload = JsonSerializer.Serialize(message);

            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic($"producer/{producerBinding.MacAddress}")
                .WithPayload(Encoding.UTF8.GetBytes(payload))
                .Build();

            await _mqttClient.PublishAsync(mqttMessage, cancellationToken);
        }
    }
}
