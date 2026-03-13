using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

public class ArticleQueuePublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public ArticleQueuePublisher(){
        var factory = new ConnectionFactory()
        {
            HostName = "rabbitmq"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(
            queue: "article-queue",
            durable: false,
            exclusive: false,
            autoDelete: false);
    }

    public void Publish(ArticleMessage message){
    var props = _channel.CreateBasicProperties();
    
    var context = Activity.Current?.Context ?? default;
    Propagators.DefaultTextMapPropagator.Inject(new PropagationContext(context, Baggage.Current), props, (carrier, key, value) =>
    {
        carrier.Headers ??= new Dictionary<string, object>();
        carrier.Headers[key] = value;
    });

    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

    _channel.BasicPublish(
        exchange: "",
        routingKey: "article-queue",
        basicProperties: props, 
        body: body);
}
}