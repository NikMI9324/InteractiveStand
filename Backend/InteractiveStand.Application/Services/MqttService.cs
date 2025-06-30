using InteractiveStand.Application.EspMessages;
using InteractiveStand.Application.Hubs;
using InteractiveStand.Application.Interfaces;
using InteractiveStand.Domain.Classes;
using InteractiveStand.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MQTTnet;
using MQTTnet.Packets;
using System.Text;
using System.Text.Json;

namespace InteractiveStand.Application.Services
{
    public class MqttService : BackgroundService, IMqttService
    {
        private readonly IMqttClient _mqttClient;
        private readonly MqttClientOptions _mqttOptions;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public MqttService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;

            var factory = new MqttClientFactory();
            _mqttClient = factory.CreateMqttClient();

            _mqttOptions = new MqttClientOptionsBuilder()
                                .WithTcpServer("192.168.205.183", 1883)
                                .WithCredentials("hello", "hello")
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
            await StartAsync(stoppingToken);
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _mqttClient.ConnectAsync(_mqttOptions, cancellationToken);
                var topicFilters = new List<MqttTopicFilter>
                {
                    new MqttTopicFilterBuilder().WithTopic("esp/update").Build(),
                    new MqttTopicFilterBuilder().WithTopic("esp/connect").Build()
                };

                await _mqttClient.SubscribeAsync(new MqttClientSubscribeOptions
                {
                    TopicFilters = topicFilters
                }, cancellationToken);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_mqttClient.IsConnected)
            {
                await _mqttClient.DisconnectAsync(cancellationToken: cancellationToken);
            }
        }

        public async Task PublishProducedCapacityAsync(ProducerBinding producerBinding, PowerSource powerSource, CancellationToken cancellationToken)
        {
            if (!_mqttClient.IsConnected || string.IsNullOrWhiteSpace(producerBinding.MacAddress))
                return;
            var currentCapacity = powerSource.GetProducerCapacity(producerBinding.CapacityProducerType);
            var message = new { power = currentCapacity };
            var payload = JsonSerializer.Serialize(message);

            await _mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"producer/{producerBinding.MacAddress}")
                .WithPayload(Encoding.UTF8.GetBytes(payload))
                .Build(), cancellationToken);
        }
    }
}
