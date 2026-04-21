public class SubscriberService {
    private readonly SubscriberDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SubscriberService> _logger;
    private readonly SubscriberQueuePublisher _publisher;

    public SubscriberService(
        SubscriberDbContext dbContext, 
        IConfiguration configuration, 
        ILogger<SubscriberService> logger,
        SubscriberQueuePublisher publisher) 
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
        _publisher = publisher;
    }

    public bool IsEnabled() => _configuration.GetValue<bool>("FeatureManagement:SubscriberServiceEnabled");

    public async Task<bool> SubscribeAsync(string email) {
        
        var subscriber = new Subscriber {
            Id = Guid.NewGuid(),
            Email = email,
            SubscribedAt = DateTime.UtcNow
        };

      
        _dbContext.Subscribers.Add(subscriber);
        await _dbContext.SaveChangesAsync();

       
        try {
            var message = new SubscriberMessage { Id = subscriber.Id, Email = subscriber.Email };
            _publisher.Publish(message);
            _logger.LogInformation($"Successfully published subscriber {email} to queue.");
        } 
        catch (Exception ex) {
          
            _logger.LogError(ex, $"Fault Isolation Active: Saved {email} to DB, but failed to reach RabbitMQ.");
        }

        return true;
    }
}