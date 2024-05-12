using Realworlddotnet.Core.Dto;

namespace Realworlddotnet.Api.Features.Articles;

public class ArticlesRoutes : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var unAuthorizedGroup = app.MapGroup("articles")
            .WithTags("Articles")
            .IncludeInOpenApi();

        var authorizedGroup = app.MapGroup("articles")
            .RequireAuthorization()
            .WithTags("Articles")
            .IncludeInOpenApi();

        unAuthorizedGroup.MapGet("/", GetArticles);
        unAuthorizedGroup.MapGet("/{slug}", GetArticleBySlug);
        unAuthorizedGroup.MapGet("/{slug}/comments", GetComments);
        
        authorizedGroup.MapPost("/", CreateArticle).Produces(StatusCodes.Status401Unauthorized);
        authorizedGroup.MapPut("/{slug}", UpdateArticle).Produces(StatusCodes.Status401Unauthorized);
        authorizedGroup.MapDelete("/{slug}", DeleteArticle).Produces(StatusCodes.Status401Unauthorized);
        authorizedGroup.MapPost("/{slug}/favorite", FavoriteBySlug).Produces(StatusCodes.Status401Unauthorized);
        authorizedGroup.MapDelete("/{slug}/favorite", UnfavoriteBySlug).Produces(StatusCodes.Status401Unauthorized);
        authorizedGroup.MapGet("/feed", GetFeed).Produces(StatusCodes.Status401Unauthorized);
        authorizedGroup.MapPost("{slug}/comments", CreateComment).Produces(StatusCodes.Status401Unauthorized);
        authorizedGroup.MapDelete("{slug}/comments/{commentId:int}", DeleteComment).Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<Ok<ArticlesResponse>> GetArticles(
        [AsParameters] ArticlesQuery query,
        IArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var user = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        var response = await articlesHandler.GetArticlesAsync(query, user, false, cancellationToken);
        var result = ArticlesMapper.MapFromArticles(response);
        return TypedResults.Ok(result);
    }
    
    private static async Task<Ok<ArticleEnvelope<ArticleResponse>>> GetArticleBySlug(
        string slug,
        IArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var user = claimsPrincipal.GetUsername();
        var article = await articlesHandler.GetArticleBySlugAsync(slug, user, cancellationToken);
        var result = ArticlesMapper.MapFromArticleEntity(article);
        return TypedResults.Ok(new ArticleEnvelope<ArticleResponse>(result));
    }
    
    private static async Task<Ok> DeleteComment(
        string slug,
        int commentId,
        IArticlesHandler articlesHandler,
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
        IArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken)
    {
        if (!MiniValidator.TryValidate(request, out var errors))
            return TypedResults.ValidationProblem(errors);

        var user = claimsPrincipal.GetUsername();
        var result = await articlesHandler.AddCommentAsync(slug, user, request.Comment, cancellationToken);
        var comment = CommentMapper.MapFromCommentEntity(result);
        return TypedResults.Ok(new CommentEnvelope<Comment>(comment));
    }

    private static async Task<Ok<ArticlesResponse>> GetFeed(
        [AsParameters] FeedQuery query,
        IArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var user = claimsPrincipal.GetUsername();
        var articlesQuery = new ArticlesQuery(null, null, null, query.Limit, query.Offset);
        var response = await articlesHandler.GetArticlesAsync(articlesQuery, user, false, cancellationToken);
        var result = ArticlesMapper.MapFromArticles(response);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<ArticleEnvelope<ArticleResponse>>> UnfavoriteBySlug(
        string slug,
        IArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var user = claimsPrincipal.GetUsername();
        var article = await articlesHandler.DeleteFavorite(slug, user, cancellationToken);
        var result = ArticlesMapper.MapFromArticleEntity(article);
        return TypedResults.Ok(new ArticleEnvelope<ArticleResponse>(result));
    }

    private static async Task<Ok<ArticleEnvelope<ArticleResponse>>> FavoriteBySlug(
        string slug,
        IArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal, 
        CancellationToken cancellationToken)
    {
        var user = claimsPrincipal.GetUsername();
        var article = await articlesHandler.AddFavoriteAsync(slug, user, cancellationToken);
        var result = ArticlesMapper.MapFromArticleEntity(article);
        return TypedResults.Ok(new ArticleEnvelope<ArticleResponse>(result));
    }

    private static async Task<Ok> DeleteArticle(
        string slug,
        IArticlesHandler articlesHandler,
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
        IArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal, 
        CancellationToken cancellationToken)
    {
        if (!MiniValidator.TryValidate(request, out var errors))
            return TypedResults.ValidationProblem(errors);
        
        var user = claimsPrincipal.GetUsername();
        var article = await articlesHandler.UpdateArticleAsync(request.Article, slug, user, cancellationToken);
        var result = ArticlesMapper.MapFromArticleEntity(article);
        return TypedResults.Ok(new ArticleEnvelope<ArticleResponse>(result));
    }

    private static async Task<Results<Ok<ArticleEnvelope<ArticleResponse>>, ValidationProblem>> CreateArticle(
        ArticleEnvelope<NewArticleDto> request,
        IArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        if (!MiniValidator.TryValidate(request, out var errors))
            return TypedResults.ValidationProblem(errors);

        var user = claimsPrincipal.GetUsername();
        var article = await articlesHandler.CreateArticleAsync(request.Article, user, cancellationToken);
        var result = ArticlesMapper.MapFromArticleEntity(article);
        return TypedResults.Ok(new ArticleEnvelope<ArticleResponse>(result));
    }

    private static async Task<Ok<CommentsEnvelope<List<Comment>>>> GetComments(
        string slug,
        IArticlesHandler articlesHandler, 
        ClaimsPrincipal claimsPrincipal, 
        CancellationToken cancellationToken)
    {
        var user = claimsPrincipal.GetUsername();
        var result = await articlesHandler.GetCommentsAsync(slug, user, cancellationToken);
        var comments = result.Select(CommentMapper.MapFromCommentEntity);
        return TypedResults.Ok(new CommentsEnvelope<List<Comment>>(comments.ToList()));
    }
}
