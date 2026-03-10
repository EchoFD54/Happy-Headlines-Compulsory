public class Article
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public DateTime PublishedAt { get; set; }

    public bool IsPublished { get; set; }

    public Continent Continent { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;
}