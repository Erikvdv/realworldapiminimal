using Microsoft.AspNetCore.Mvc;
using Realworlddotnet.Core.Dto;
using Realworlddotnet.Core.Repositories;

namespace Realworlddotnet.Api.Features.Articles;

public class ArticlesHandler(IConduitRepository repository)
{
    public async Task<Article> CreateArticleAsync(
        NewArticleDto newArticle, string username, CancellationToken cancellationToken)
    {
        var user = await repository.GetUserByUsernameAsync(username, cancellationToken);
        var tags = await repository.UpsertTagsAsync(newArticle.TagList, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        var article = new Article(
                newArticle.Title,
                newArticle.Description,
                newArticle.Body
            ) { Author = user, Tags = tags.ToList() }
            ;

        repository.AddArticle(article);
        await repository.SaveChangesAsync(cancellationToken);
        return article;
    }

    public async Task<Article> UpdateArticleAsync(
        ArticleUpdateDto update, string slug, string username, CancellationToken cancellationToken)
    {
        var article = await repository.GetArticleBySlugAsync(slug, false, cancellationToken);

        if (article == null)
        {
            throw new KeyNotFoundException("ArticleNotFound");
        }

        if (username != article.Author.Username)
        {
            throw new UnauthorizedAccessException($"{username} is not the author");
        }

        article.UpdateArticle(update);
        await repository.SaveChangesAsync(cancellationToken);
        return article;
    }

    public async Task DeleteArticleAsync(string slug, string username, CancellationToken cancellationToken)
    {

        var article = await repository.GetArticleBySlugAsync(slug, false, cancellationToken) ??
                      throw new KeyNotFoundException("Article not found");

        if (username != article.Author.Username)
        {
            throw new UnauthorizedAccessException($"{username} is not the author");
        }

        repository.DeleteArticle(article);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public Task<ArticlesResponseDto> GetArticlesAsync(ArticlesQuery query, string? username, bool isFeed,
        CancellationToken cancellationToken)
    {
        var getArticlesQueryDto = query.MapToArticlesQueryDto();
        return repository.GetArticlesAsync(getArticlesQueryDto, username, false, cancellationToken);
    }


    public async Task<Article> GetArticleBySlugAsync(string slug, string? username, CancellationToken cancellationToken)
    {
        var article = await repository.GetArticleBySlugAsync(slug, false, cancellationToken) ??
                      throw new KeyNotFoundException("Article not found");

        var comments = await repository.GetCommentsBySlugAsync(slug, username, cancellationToken);
        article.Comments = comments;

        return article;
    }

    public async Task<Core.Entities.Comment> AddCommentAsync(string slug, string username, CommentDto commentDto,
        CancellationToken cancellationToken)
    {
        var user = await repository.GetUserByUsernameAsync(username, cancellationToken);
        var article = await repository.GetArticleBySlugAsync(slug, false, cancellationToken) ??
                      throw new KeyNotFoundException("Article not found");

        var comment = new Core.Entities.Comment(commentDto.Body, user.Username, article.Id);
        repository.AddArticleComment(comment);

        await repository.SaveChangesAsync(cancellationToken);
        return comment;
    }

    public async Task RemoveCommentAsync(string slug, int commentId, string username,
        CancellationToken cancellationToken)
    {
        _ = await repository.GetArticleBySlugAsync(slug, false, cancellationToken) ??
            throw new KeyNotFoundException("Article not found");

        var comments = await repository.GetCommentsBySlugAsync(slug, username, cancellationToken);
        var comment = comments.Find(x => x.Id == commentId)
                      ?? throw new KeyNotFoundException("Comment not found");;


        if (comment.Author.Username != username)
        {
            throw new UnauthorizedAccessException("User does not own Article");
        }

        comments.Remove(comment);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Core.Entities.Comment>> GetCommentsAsync(string slug, string? username,
        CancellationToken cancellationToken)
    {
        var comments = await repository.GetCommentsBySlugAsync(slug, username, cancellationToken);
        return comments;
    }

    public async Task<Article> AddFavoriteAsync(string slug, string username, CancellationToken cancellationToken)
    {
        var user = await repository.GetUserByUsernameAsync(username, cancellationToken);
        var article = await repository.GetArticleBySlugAsync(slug, false, cancellationToken) ??
                      throw new KeyNotFoundException("Article not found");

        var articleFavorite = await repository.GetArticleFavoriteAsync(user.Username, article.Id);

        if (articleFavorite is null)
        {
            repository.AddArticleFavorite(new ArticleFavorite(user.Username, article.Id));
            await repository.SaveChangesAsync(cancellationToken);
        }

        article = await repository.GetArticleBySlugAsync(slug, false, cancellationToken);
        return article!;
    }

    public async Task<Article> DeleteFavorite(string slug, string username, CancellationToken cancellationToken)
    {
        var user = await repository.GetUserByUsernameAsync(username, cancellationToken);
        var article = await repository.GetArticleBySlugAsync(slug, false, cancellationToken) ??
                      throw new KeyNotFoundException("Article not found");

        var articleFavorite = await repository.GetArticleFavoriteAsync(user.Username, article.Id);

        if (articleFavorite is not null)
        {
            repository.RemoveArticleFavorite(articleFavorite);
            await repository.SaveChangesAsync(cancellationToken);
        }

        article = await repository.GetArticleBySlugAsync(slug, false, cancellationToken);
        return article!;
    }
}
