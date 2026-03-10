public interface IArticleDbContextRouter
{
    ArticleDbContext GetDbContext(Continent continent);
}