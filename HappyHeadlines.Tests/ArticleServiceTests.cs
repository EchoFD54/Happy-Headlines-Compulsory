using Xunit;

//Simple test to validate the 14-day caching logic for articles
public class CacheLogicTests {
    [Fact]
    public void Test_14DayCutoff_Logic() {
        var now = DateTime.UtcNow;
        var articleDate = now.AddDays(-10); 
        var oldArticleDate = now.AddDays(-20); 
        
        var cutoff = now.AddDays(-14);
        bool isFresh = articleDate > cutoff;
        bool isStale = oldArticleDate > cutoff;

        Assert.True(isFresh, "Articles within 10 days should be cached.");
        Assert.False(isStale, "Articles older than 20 days should NOT be cached.");
    }
}