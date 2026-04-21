using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

public class SubscriberMessage {
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
}

public class Worker : BackgroundService{
    private readonly ILogger<Worker> _logger;
    private static readonly ActivitySource ActivitySource = new("HappyHeadlines.NewsletterService");

    public Worker(ILogger<Worker> logger){
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken){
        var factory = new ConnectionFactory() { HostName = "rabbitmq" };
        IConnection? connection = null;
        var retries = 0;

        while (connection == null && retries < 10) {
            try { connection = factory.CreateConnection(); }
            catch {
                retries++;
                _logger.LogWarning($"RabbitMQ not ready. Retrying... ({retries}/10)");
                await Task.Delay(3000, stoppingToken);
            }
        }

        if (connection == null) {
            _logger.LogError("Failed to connect to RabbitMQ.");
            return;
        }

        var channel = connection.CreateModel();
        channel.QueueDeclare(queue: "subscriber-queue", durable: false, exclusive: false, autoDelete: false);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>{
           
            var parentContext = Propagators.DefaultTextMapPropagator.Extract(default, ea.BasicProperties, (carrier, key) =>
            {
                if (carrier.Headers != null && carrier.Headers.TryGetValue(key, out var value)) {
                    return new[] { Encoding.UTF8.GetString(value as byte[] ?? Array.Empty<byte>()) };
                }
                return Enumerable.Empty<string>();
            });

            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var message = JsonSerializer.Deserialize<SubscriberMessage>(json);

            if (message != null) {
                using var activity = ActivitySource.StartActivity("ProcessSubscriber", ActivityKind.Consumer, parentContext.ActivityContext);
                
                _logger.LogInformation($"[EMAIL SENT] Welcome to HappyHeadlines, {message.Email}!");
            }
        };

        channel.BasicConsume(queue: "subscriber-queue", autoAck: true, consumer: consumer);
        _logger.LogInformation("Newsletter Service is listening for new subscribers...");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}