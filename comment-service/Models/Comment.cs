using System;

public class Comment
{
    public Guid Id { get; set; } 

    public Guid ArticleId { get; set; } 
    public Guid AuthorId { get; set; } 

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsApproved { get; set; } = false; 
}