using Realworlddotnet.Core.Dto;
using Realworlddotnet.Infrastructure.Extensions.OpenApi;

namespace Realworlddotnet.Api.Features.Articles;

public static class ArticlesEndpoints
{
    public static void AddArticlesEndpoints(this IEndpointRouteBuilder app)
    {
        var unAuthorizedGroup = app.MapGroup("articles").WithTags("Articles");

        var authorizedGroup = app.MapGroup("articles").RequireAuthorization().WithTags("Articles")
            .WithUnauthenticated();

        unAuthorizedGroup.MapGet("/", GetArticles);
        unAuthorizedGroup.MapGet("/{slug}", GetArticleBySlug);
        unAuthorizedGroup.MapGet("/{slug}/comments", GetComments);

        authorizedGroup.MapPost("/", CreateArticle);
        authorizedGroup.MapPut("/{slug}", UpdateArticle);
        authorizedGroup.MapDelete("/{slug}", DeleteArticle);
        authorizedGroup.MapPost("/{slug}/favorite", FavoriteBySlug);
        authorizedGroup.MapDelete("/{slug}/favorite", UnfavoriteBySlug);
        authorizedGroup.MapGet("/feed", GetFeed);
        authorizedGroup.MapPost("{slug}/comments", CreateComment);
        authorizedGroup.MapDelete("{slug}/comments/{commentId:int}", DeleteComment);
    }

    private static async Task<Ok<ArticlesResponse>> GetArticles(
        [AsParameters] ArticlesQuery query,
        ArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var user = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        var response = await articlesHandler.GetArticlesAsync(query, user, false, cancellationToken);
        var result = response.MapToArticlesResponse();
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<ArticleEnvelope<ArticleResponse>>> GetArticleBySlug(
        string slug,
        ArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var user = claimsPrincipal.GetUsername();
        var article = await articlesHandler.GetArticleBySlugAsync(slug, user, cancellationToken);
        var result = article.MapToArticleResponse();
        return TypedResults.Ok(new ArticleEnvelope<ArticleResponse>(result));
    }

    private static async Task<Ok> DeleteComment(
        string slug,
        int commentId,
        ArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var user = claimsPrincipal.GetUsername();
        await articlesHandler.RemoveCommentAsync(slug, commentId, user, cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok<CommentEnvelope<Comment>>, ValidationProblem>> CreateComment(
        string slug,
        CommentEnvelope<CommentDto> request,
        ArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken)
    {
        if (!MiniValidator.TryValidate(request, out var errors))
        {
            return TypedResults.ValidationProblem(errors);
        }

        var user = claimsPrincipal.GetUsername();
        var result = await articlesHandler.AddCommentAsync(slug, user, request.Comment, cancellationToken);
        var comment = CommentMapper.MapFromCommentEntity(result);
        return TypedResults.Ok(new CommentEnvelope<Comment>(comment));
    }

    private static async Task<Ok<ArticlesResponse>> GetFeed(
        [AsParameters] FeedQuery query,
        ArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var user = claimsPrincipal.GetUsername();
        var articlesQuery = new ArticlesQuery(null, null, null, query.Limit, query.Offset);
        var response = await articlesHandler.GetArticlesAsync(articlesQuery, user, false, cancellationToken);
        var result = response.MapToArticlesResponse();
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<ArticleEnvelope<ArticleResponse>>> UnfavoriteBySlug(
        string slug,
        ArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var user = claimsPrincipal.GetUsername();
        var article = await articlesHandler.DeleteFavorite(slug, user, cancellationToken);
        var result = article.MapToArticleResponse();
        return TypedResults.Ok(new ArticleEnvelope<ArticleResponse>(result));
    }

    private static async Task<Ok<ArticleEnvelope<ArticleResponse>>> FavoriteBySlug(
        string slug,
        ArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var user = claimsPrincipal.GetUsername();
        var article = await articlesHandler.AddFavoriteAsync(slug, user, cancellationToken);
        var result = article.MapToArticleResponse();
        return TypedResults.Ok(new ArticleEnvelope<ArticleResponse>(result));
    }

    private static async Task<Ok> DeleteArticle(
        string slug,
        ArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var user = claimsPrincipal.GetUsername();
        await articlesHandler.DeleteArticleAsync(slug, user, cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok<ArticleEnvelope<ArticleResponse>>, ValidationProblem>> UpdateArticle(
        string slug,
        ArticleEnvelope<ArticleUpdateDto> request,
        ArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        if (!MiniValidator.TryValidate(request, out var errors))
        {
            return TypedResults.ValidationProblem(errors);
        }

        var user = claimsPrincipal.GetUsername();
        var article = await articlesHandler.UpdateArticleAsync(request.Article, slug, user, cancellationToken);
        var result = article.MapToArticleResponse();
        return TypedResults.Ok(new ArticleEnvelope<ArticleResponse>(result));
    }

    private static async Task<Results<Ok<ArticleEnvelope<ArticleResponse>>, ValidationProblem>> CreateArticle(
        ArticleEnvelope<NewArticleDto> request,
        ArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        if (!MiniValidator.TryValidate(request, out var errors))
        {
            return TypedResults.ValidationProblem(errors);
        }

        var user = claimsPrincipal.GetUsername();
        var article = await articlesHandler.CreateArticleAsync(request.Article, user, cancellationToken);
        var result = article.MapToArticleResponse();
        return TypedResults.Ok(new ArticleEnvelope<ArticleResponse>(result));
    }

    private static async Task<Ok<CommentsEnvelope<List<Comment>>>> GetComments(
        string slug,
        ArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var user = claimsPrincipal.GetUsername();
        var result = await articlesHandler.GetCommentsAsync(slug, user, cancellationToken);
        var comments = result.Select(CommentMapper.MapFromCommentEntity);
        return TypedResults.Ok(new CommentsEnvelope<List<Comment>>(comments.ToList()));
    }
}
