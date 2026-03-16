using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

public class ArticleQueueConsumer : BackgroundService{
    private readonly IServiceProvider _serviceProvider;

    private static readonly ActivitySource ActivitySource =
        new("HappyHeadlines.ArticleService");

    public ArticleQueueConsumer(IServiceProvider serviceProvider){
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken){
        var factory = new ConnectionFactory(){
            HostName = "rabbitmq"
        };

          IConnection? connection = null;
            var retries = 0;
            while (connection == null && retries < 10){
                try
                {
                    connection = factory.CreateConnection();
                }
                catch (Exception)
                {
                    retries++;
                    await Task.Delay(3000, stoppingToken);
                }
            }

            if (connection == null){
                Console.WriteLine("Failed to connect to RabbitMQ after 10 retries.");
                return;
            }

    var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: "article-queue",
            durable: false,
            exclusive: false,
            autoDelete: false);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (model, ea) => {
    
    var parentContext = Propagators.DefaultTextMapPropagator.Extract(default, ea.BasicProperties, (carrier, key) =>
    {
        try
        {
            if (carrier.Headers != null && carrier.Headers.TryGetValue(key, out var value))
            {
                
                var bytes = value as byte[];
                return new[] { Encoding.UTF8.GetString(bytes) };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to extract tracing header: {ex.Message}");
        }
        return Enumerable.Empty<string>();
    });

    var body = ea.Body.ToArray();
    var json = Encoding.UTF8.GetString(body);
    var message = JsonSerializer.Deserialize<ArticleMessage>(json);

    if (message == null) return;

    
    using var activity = ActivitySource.StartActivity(
        "ProcessArticleMessage",
        ActivityKind.Consumer,
        parentContext.ActivityContext); 
        Console.WriteLine($"Tracing: TraceId={activity?.TraceId}, ParentId={activity?.ParentId}");

        if (activity == null) {
            Console.WriteLine("Activity failed to start");
        }

    activity?.SetTag("article.title", message.Title);

    using var scope = _serviceProvider.CreateScope();
    var service = scope.ServiceProvider.GetRequiredService<ArticleService>();

    await service.CreateArticle(new Article {
        Id = message.Id,
        Title = message.Title,
        Content = message.Content,
        Author = message.Author,
        PublishedAt = message.PublishedAt,
        Category = message.Category,
        Summary = message.Summary,
        Continent = (Continent)message.Continent,
        IsPublished = true
    });
};

        channel.BasicConsume(
            queue: "article-queue",
            autoAck: true,
            consumer: consumer);

            Console.WriteLine("Consumer is listening...");

         await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}