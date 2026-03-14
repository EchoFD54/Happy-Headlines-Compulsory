using StackExchange.Redis;
using System.Text.Json;

public class ArticleCachePreloader : BackgroundService{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDatabase _cache;
    private readonly TimeSpan _refreshInterval = TimeSpan.FromMinutes(10); 

    public ArticleCachePreloader(IServiceProvider serviceProvider, IConnectionMultiplexer redis){
        _serviceProvider = serviceProvider;
        _cache = redis.GetDatabase();
    }

   protected override async Task ExecuteAsync(CancellationToken stoppingToken){
    while (!stoppingToken.IsCancellationRequested){
        try{
            using var scope = _serviceProvider.CreateScope();
            var router = scope.ServiceProvider.GetRequiredService<IArticleDbContextRouter>();
            var cutoffDate = DateTime.UtcNow.AddDays(-14);

            var validArticleIds = new HashSet<string>();

            
            foreach (Continent continent in Enum.GetValues(typeof(Continent))){
                var db = router.GetDbContext(continent);
                var recentArticles = db.Articles
                    .Where(a => a.PublishedAt >= cutoffDate)
                    .ToList();

                foreach (var article in recentArticles)
                {
                    string key = $"article:{article.Id}";
                    validArticleIds.Add(key); 
                    string value = JsonSerializer.Serialize(article);
                    await _cache.StringSetAsync(key, value, TimeSpan.FromHours(24));
                }
            }

            var server = _cache.Multiplexer.GetServer(_cache.Multiplexer.GetEndPoints()[0]);
            var allCachedKeys = server.Keys(pattern: "article:*").Select(k => k.ToString());

            foreach (var cachedKey in allCachedKeys){
                if (!validArticleIds.Contains(cachedKey)){
                    await _cache.KeyDeleteAsync(cachedKey);
                    Console.WriteLine($"Preloader: Removed expired article {cachedKey} from cache.");
                }
            }

            Console.WriteLine($"Cache Preloader: Sync complete at {DateTime.Now}");
        }
        catch (Exception ex){
            Console.WriteLine($"Cache Preloader Error: {ex.Message}");
        }

        await Task.Delay(_refreshInterval, stoppingToken);
    }
}
}