namespace Realworlddotnet.Api.Features.Articles;

public record ArticleEnvelope<T>(T Article);

public record CommentEnvelope<T>(T Comment);

public record CommentsEnvelope<T>(T Comments);

public record Comment(
    int Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string Body,
    Author Author);

public record Author(string Username, string Image, string Bio, bool Following);

public record ArticleResponse(
    string Slug,
    string Title,
    string Description,
    string Body,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IEnumerable<string> TagList,
    Author Author,
    bool Favorited,
    int FavoritesCount);

public record ArticlesResponse(
    IEnumerable<ArticleResponse> Articles,
    int ArticlesCount);

public record NewArticleDto(
    [Required] string Title,
    [Required] string Description,
    [Required] string Body,
    [Required] IEnumerable<string> TagList);

public record ArticlesQuery(string? Tag, string? Author, string? Favorited, int Limit = 20, int Offset = 0);

public record FeedQuery(int Limit = 20, int Offset = 0);

public record CommentDto(string Body);
