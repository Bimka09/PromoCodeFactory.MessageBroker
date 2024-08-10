using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Threading.Tasks;
using System.Threading;
using Otus.Teaching.Pcf.Administration.WebHost.Settings;
using System.Text;
using System.Text.Json;
using Otus.Teaching.Pcf.Administration.WebHost.Models;
using Otus.Teaching.Pcf.Administration.Core.Abstractions.Services;

namespace Otus.Teaching.Pcf.Administration.WebHost.HostedServices
{
    public class PromocodeConsumerService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private const string Exchange = "Pcf.ReceivingFromPartner.Promocodes";
        private const string Queue = "Pcf.Administration.Promocodes";
        private const string RoutingKey = "Pcf.ReceivingFromPartner.Promocode";
        private readonly TaskCompletionSource _source = new();

        private readonly ConnectionFactory _connectionFactory;
        private IModel _channel;
        private IConnection _con;
        private readonly IHostApplicationLifetime _lifetime;

        public PromocodeConsumerService(IServiceScopeFactory serviceScopeFactory, IHostApplicationLifetime lifetime,
            IOptions<RmqSettings> rmqSettings)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _lifetime = lifetime;
            _lifetime.ApplicationStarted.Register(() => _source.SetResult());
            _connectionFactory = new ConnectionFactory
            {
                UserName = rmqSettings.Value.Login,
                Password = rmqSettings.Value.Password,
                HostName = rmqSettings.Value.Host,
                VirtualHost = rmqSettings.Value.VHost
            };
        }

        private IModel InitConsumer()
        {
            _con = _connectionFactory.CreateConnection();
            _channel = _con.CreateModel();
            _channel.QueueDeclare(
                queue: Queue,
                exclusive: false,
                durable: true,
                autoDelete: false);


            _channel.ExchangeDeclare(
                exchange: Exchange,
                type: ExchangeType.Direct,
                durable: true);

            _channel.QueueBind(
                queue: Queue,
                exchange: Exchange,
                routingKey: RoutingKey);

            return _channel;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!await WaitForAppStartup(_lifetime, stoppingToken))
            {
                return;
            }
            var initConsumer = InitConsumer();
            var consumer = new EventingBasicConsumer(initConsumer);

            consumer.Received += async (s, ea) =>
            {
                await using var scope = _serviceScopeFactory.CreateAsyncScope();
                {
                    var employeeService = scope.ServiceProvider.GetRequiredService<IEmployeeService>();
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var promocodeDto = JsonSerializer.Deserialize<PromocodeDto>(message);
                    await employeeService.UpdateAppliedPromocodes(promocodeDto.PartnerManagerId);

                    initConsumer.BasicAck(ea.DeliveryTag, false);
                }
                ;
            };
            initConsumer.BasicConsume(queue: Queue, autoAck: false, consumer: consumer);
        }

        static async Task<bool> WaitForAppStartup(IHostApplicationLifetime lifetime, CancellationToken stoppingToken)
        {
            var startedSource = new TaskCompletionSource();
            var cancelledSource = new TaskCompletionSource();

            await using var reg1 = lifetime.ApplicationStarted.Register(() => startedSource.SetResult());
            await using var reg2 = stoppingToken.Register(() => cancelledSource.SetResult());

            var completedTask = await Task.WhenAny(
                startedSource.Task,
                cancelledSource.Task).ConfigureAwait(false);

            return completedTask == startedSource.Task;
        }
    }
}
