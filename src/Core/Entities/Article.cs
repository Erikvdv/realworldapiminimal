using System.ComponentModel.DataAnnotations;
using Realworlddotnet.Core.Dto;
using Realworlddotnet.Infrastructure.Utils;

namespace Realworlddotnet.Core.Entities;

public class Article(string title, string description, string body)
{
    public Guid Id { get; set; }

    [MaxLength(45)]
    public string Slug { get; set; } = title.GenerateSlug();

    [MaxLength(200)]
    public string Title { get; set; } = title;

    [MaxLength(500)]
    public string Description { get; set; } = description;

    [MaxLength(2000)]
    public string Body { get; set; } = body;

    public User Author { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool Favorited { get; set; }

    public int FavoritesCount { get; set; } = 0;

    public ICollection<Tag> Tags { get; set; } = new List<Tag>();

    public List<Comment> Comments { get; set; } = new();
    public ICollection<ArticleFavorite> ArticleFavorites { get; set; } = new List<ArticleFavorite>();

    public void UpdateArticle(ArticleUpdateDto update)
    {
        if (!string.IsNullOrWhiteSpace(update.Title))
        {
            Title = update.Title;
            Slug = update.Title.GenerateSlug();
        }

        if (!string.IsNullOrWhiteSpace(update.Body))
        {
            Body = update.Body;
        }

        if (!string.IsNullOrWhiteSpace(update.Description))
        {
            Description = update.Description;
        }

        UpdatedAt = DateTime.UtcNow;
    }
}
