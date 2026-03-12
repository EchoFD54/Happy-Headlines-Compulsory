public class Draft{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public Guid AuthorId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}