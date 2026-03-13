public class ArticleMessage
{
    

    public Guid Id { get; set; }

    public string Title { get; set; } = "";

    public string Content { get; set; } = "";

    public string Author { get; set; } = "";

    public DateTime PublishedAt { get; set; }

    public string Category { get; set; } = "";

    public string Summary { get; set; } = "";

    public int Continent { get; set; }

}