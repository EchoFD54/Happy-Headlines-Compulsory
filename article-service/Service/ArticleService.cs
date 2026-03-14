using StackExchange.Redis;
using System.Diagnostics.Metrics;
using System.Text.Json;

public class ArticleService{
    private readonly IArticleDbContextRouter _router;
    private readonly IDatabase _cache;

    private readonly Counter<long> _cacheHitCounter;
    private readonly Counter<long> _cacheMissCounter;

    public ArticleService(IArticleDbContextRouter router, IConnectionMultiplexer redis) {
        _router = router;
        _cache = redis.GetDatabase();

        var meter = new Meter("HappyHeadlines.ArticleService");
        _cacheHitCounter = meter.CreateCounter<long>("article_cache_hits");
        _cacheMissCounter = meter.CreateCounter<long>("article_cache_misses");
    }

    public async Task<Article> CreateArticle(Article article){
        var db = _router.GetDbContext(article.Continent);

        db.Articles.Add(article);
        await db.SaveChangesAsync();

        return article;
    }

    public async Task<Article?> GetArticle(Guid id, Continent continent) {
        string key = $"article:{id}";
        var cachedArticle = await _cache.StringGetAsync(key);

        if (cachedArticle.HasValue) {
            _cacheHitCounter.Add(1);
            Console.WriteLine("Cache hit, returning article from Redis");
            return JsonSerializer.Deserialize<Article>((string?)cachedArticle!);
        }

        _cacheMissCounter.Add(1);
        Console.WriteLine("Cache missed, fetching from database...");
        
        var db = _router.GetDbContext(continent);
        var article = await db.Articles.FindAsync(id);

        if (article != null) {
            await _cache.StringSetAsync(key, JsonSerializer.Serialize(article), TimeSpan.FromHours(1));
        }

        return article;
    }

    public async Task<Article?> UpdateArticle(Guid id, Article updatedArticle){
        var db = _router.GetDbContext(updatedArticle.Continent);

        var existing = await db.Articles.FindAsync(id);
        if (existing == null) return null;

        existing.Title = updatedArticle.Title;
        existing.Content = updatedArticle.Content;
        existing.Author = updatedArticle.Author;
        existing.PublishedAt = updatedArticle.PublishedAt;
        existing.IsPublished = updatedArticle.IsPublished;
        existing.Category = updatedArticle.Category;
        existing.Summary = updatedArticle.Summary;

        await db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteArticle(Guid id, Continent continent) {
        var db = _router.GetDbContext(continent);
        var article = await db.Articles.FindAsync(id);
        if (article == null) return false;
        db.Articles.Remove(article);
        await db.SaveChangesAsync();
        await _cache.KeyDeleteAsync($"article:{id}"); 
        return true;
    }
    }