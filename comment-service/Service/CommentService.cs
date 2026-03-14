using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Diagnostics.Metrics;
using System.Text.Json;

public class CommentService {
    private readonly CommentDbContext _dbContext;
    private readonly ProfanityServiceClient _profanityClient;
    private readonly IDatabase _cache;

    private readonly Counter<long> _hitCounter;
    private readonly Counter<long> _missCounter;
    private const string LRU_KEY = "comment_lru";
    private const int MAX_CACHE_SIZE = 30;

    public CommentService(CommentDbContext dbContext, ProfanityServiceClient profanityClient, IConnectionMultiplexer redis) {
        _dbContext = dbContext;
        _profanityClient = profanityClient;
        _cache = redis.GetDatabase();

        var meter = new Meter("HappyHeadlines.CommentService");
        _hitCounter = meter.CreateCounter<long>("comment_cache_hits");
        _missCounter = meter.CreateCounter<long>("comment_cache_misses");
    }

    public async Task<List<Comment>> GetCommentsByArticle(Guid articleId) {
        string cacheKey = $"comments:{articleId}";

        var cachedData = await _cache.StringGetAsync(cacheKey);
        if (cachedData.HasValue) {
            _hitCounter.Add(1);
            Console.WriteLine($"⚡ Comment Cache Hit: Article {articleId}");
            
            await UpdateLruAccess(articleId.ToString());
            
            return JsonSerializer.Deserialize<List<Comment>>((string?)cachedData!)!;
        }

        _missCounter.Add(1);
        Console.WriteLine($"Comment Cache Miss: Article {articleId}");
        var comments = await _dbContext.Comments
            .Where(c => c.ArticleId == articleId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        if (comments.Any()) {
            await SaveToCache(articleId.ToString(), comments);
        }

        return comments;
    }

    private async Task UpdateLruAccess(string articleId) {
        await _cache.SortedSetAddAsync(LRU_KEY, articleId, DateTime.UtcNow.Ticks);
    }

    private async Task SaveToCache(string articleId, List<Comment> comments) {
        string cacheKey = $"comments:{articleId}";
        
        await _cache.StringSetAsync(cacheKey, JsonSerializer.Serialize(comments), TimeSpan.FromHours(1));

        await UpdateLruAccess(articleId);

        long count = await _cache.SortedSetLengthAsync(LRU_KEY);
        if (count > MAX_CACHE_SIZE) {
            var oldest = await _cache.SortedSetRangeByRankAsync(LRU_KEY, 0, 0);
            if (oldest.Length > 0) {
                string oldestKey = oldest[0]!;
                await _cache.KeyDeleteAsync($"comments:{oldestKey}"); 
                await _cache.SortedSetRemoveAsync(LRU_KEY, oldestKey); 
                Console.WriteLine($"🗑️ Comment Cache LRU: Evicted article {oldestKey} to maintain limit of 30.");
            }
        }
    }

    public async Task<Comment> CreateComment(Comment comment) {
        comment.CreatedAt = DateTime.UtcNow;
        try {
            var hasProfanity = await _profanityClient.ContainsProfanity(comment.Content);
            comment.IsApproved = !hasProfanity;
        } catch {
            comment.IsApproved = false;
        }
        
        _dbContext.Comments.Add(comment);
        await _dbContext.SaveChangesAsync();

        await _cache.KeyDeleteAsync($"comments:{comment.ArticleId}");
        await _cache.SortedSetRemoveAsync(LRU_KEY, comment.ArticleId.ToString());

        return comment;
    }
}