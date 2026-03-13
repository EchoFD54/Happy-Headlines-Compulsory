using System.Diagnostics;

public class PublisherService{
    private readonly ProfanityServiceClient _profanityClient;
    private readonly ArticleQueuePublisher _queue;

    private static readonly ActivitySource ActivitySource = new("HappyHeadlines.PublisherService");

    public PublisherService(ProfanityServiceClient profanityClient,ArticleQueuePublisher queue){
        _profanityClient = profanityClient;
        _queue = queue;
    }

    public async Task PublishArticle(ArticleMessage article){
        using var activity = ActivitySource.StartActivity("PublishArticle");

        activity?.SetTag("article.title", article.Title);

        var hasProfanity = await _profanityClient.ContainsProfanity(article.Content);

        if (hasProfanity)
            throw new Exception("Article contains profanity");

    
        _queue.Publish(article);
    }
}