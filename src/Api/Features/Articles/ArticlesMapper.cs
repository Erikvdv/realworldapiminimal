using Realworlddotnet.Core.Dto;

namespace Realworlddotnet.Api.Features.Articles;

public static class ArticlesMapper
{
    public static ArticleResponse MapToArticleResponse(this Article article)
    {
        var tags = article.Tags.Select(tag => tag.Id);
        var author = article.Author;
        var result = new ArticleResponse(
            article.Slug,
            article.Title,
            article.Description,
            article.Body,
            article.CreatedAt,
            article.UpdatedAt,
            tags,
            new Author(
                author.Username,
                author.Image,
                author.Bio,
                author.Followers.Count != 0),
            article.Favorited,
            article.FavoritesCount);
        return result;
    }

    public static ArticlesResponse MapToArticlesResponse(this ArticlesResponseDto articlesResponseDto)
    {
        var articles = articlesResponseDto.Articles
            .Select(article => article.MapToArticleResponse())
            .ToList();;
        return new ArticlesResponse(articles, articlesResponseDto.ArticlesCount);
    }

    public static ArticlesQueryDto MapToArticlesQueryDto(this ArticlesQuery query)
    {
        return new ArticlesQueryDto(query.Tag, query.Author, query.Favorited, query.Limit, query.Offset);
    }
}
