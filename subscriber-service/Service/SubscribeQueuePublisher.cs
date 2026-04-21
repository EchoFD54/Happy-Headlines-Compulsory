using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

public class SubscriberQueuePublisher{
    private readonly string _hostName = "rabbitmq";

    public void Publish(SubscriberMessage message){
        var factory = new ConnectionFactory()
        {
            HostName = _hostName
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: "subscriber-queue",
            durable: false,
            exclusive: false,
            autoDelete: false);

        var props = channel.CreateBasicProperties();
        
        var context = Activity.Current?.Context ?? default;
        Propagators.DefaultTextMapPropagator.Inject(new PropagationContext(context, Baggage.Current), props, (carrier, key, value) =>
        {
            carrier.Headers ??= new Dictionary<string, object>();
            carrier.Headers[key] = value;
        });

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        channel.BasicPublish(
            exchange: "",
            routingKey: "subscriber-queue",
            basicProperties: props, 
            body: body);
    }
}