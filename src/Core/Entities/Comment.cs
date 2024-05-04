namespace Realworlddotnet.Core.Entities;

public class Comment(string body, string username, Guid articleId)
{
    public int Id { get; set; }
    public string Body { get; set; } = body;
    public string Username { get; set; } = username;
    public Guid ArticleId { get; set; } = articleId;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User Author { get; set; } = null!;
    public Article Article { get; set; } = null!;
}
