public class ArticleService{
    private readonly IArticleDbContextRouter _router;

    public ArticleService(IArticleDbContextRouter router){
        _router = router;
    }

    public async Task<Article> CreateArticle(Article article){
        var db = _router.GetDbContext(article.Continent);

        db.Articles.Add(article);
        await db.SaveChangesAsync();

        return article;
    }

    public async Task<Article?> GetArticle(Guid id, Continent continent){
    var db = _router.GetDbContext(continent);
    return await db.Articles.FindAsync(id);
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

    public async Task<bool> DeleteArticle(Guid id, Continent continent){
        var db = _router.GetDbContext(continent);
        var article = await db.Articles.FindAsync(id);
        if (article == null) return false;

        db.Articles.Remove(article);
        await db.SaveChangesAsync();
        return true;
    }
    }