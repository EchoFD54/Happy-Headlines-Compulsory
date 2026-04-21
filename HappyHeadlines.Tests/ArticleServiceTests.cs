using Xunit;
using Xunit.Abstractions; 

public class CacheLogicTests {
    private readonly ITestOutputHelper _output;

    public CacheLogicTests(ITestOutputHelper output) {
        _output = output;
    }
    //Simple test to validate the 14-day caching logic for articles
    [Fact]
    public void Test_14DayCutoff_Logic() {
        var now = DateTime.UtcNow;
        var articleDate = now.AddDays(-10); 
        var oldArticleDate = now.AddDays(-20); 
        var cutoff = now.AddDays(-14);

        _output.WriteLine($"Current Time: {now}");
        _output.WriteLine($"Cutoff Date: {cutoff}");
        _output.WriteLine($"Testing Article Date: {articleDate} (Should be Fresh)");
        _output.WriteLine($"Testing Old Article Date: {oldArticleDate} (Should be Stale)");

        bool isFresh = articleDate > cutoff;
        bool isStale = oldArticleDate > cutoff;

        Assert.True(isFresh, "Articles within 10 days should be cached.");
        Assert.False(isStale, "Articles older than 20 days should NOT be cached.");
    }
}