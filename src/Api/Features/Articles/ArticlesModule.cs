using Realworlddotnet.Core.Dto;

namespace Realworlddotnet.Api.Features.Articles;

public class ArticlesModule : ICarterModule
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

        unAuthorizedGroup.MapGet("/", GetArticles)
            .WithName("GetArticles");

        unAuthorizedGroup.MapGet("/{slug}", GetArticleBySlug)
            .WithName("GetArticle");

        unAuthorizedGroup.MapGet("/{slug}/comments", GetComments)
            .WithName("GetArticleComments");
        
        authorizedGroup.MapPost("/", CreateArticle)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("CreateArticle");

        authorizedGroup.MapPut("/{slug}", UpdateArticle)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("UpdateArticle");

        authorizedGroup.MapDelete("/{slug}", DeleteArticle)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("DeleteArticle");

        authorizedGroup.MapPost("/{slug}/favorite", FavoriteBySlug)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("FavoriteBySlug");

        authorizedGroup.MapDelete("/{slug}/favorite", UnfavoriteBySlug)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("UnFavoriteBySlug");

        authorizedGroup.MapGet("/feed", GetFeed)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("GetFeed");

        authorizedGroup.MapPost("{slug}/comments", CreateComment)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("CreateComment");

        authorizedGroup.MapDelete("{slug}/comments/{commentId:int}", 
                DeleteComment)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("DeleteArticleComment");
    }

    private static async Task<Ok<ArticlesResponse>> GetArticles(
        [AsParameters] ArticlesQuery query,
        IArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal)
    {
        var user = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        var response = await articlesHandler.GetArticlesAsync(query,
            user,
            false,
            new CancellationToken());
        var result = ArticlesMapper.MapFromArticles(response);
        return TypedResults.Ok(result);
    }
    
    private static async Task<Ok<ArticleEnvelope<ArticleResponse>>> GetArticleBySlug(string slug,
        IArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal)
    {
        var user = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        var article = await articlesHandler.GetArticleBySlugAsync(slug,
            user,
            new CancellationToken());
        var result = ArticlesMapper.MapFromArticleEntity(article);
        return TypedResults.Ok(new ArticleEnvelope<ArticleResponse>(result));
    }
    
    private static async Task<Ok> DeleteComment(string slug,
        int commentId,
        IArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal)
    {
        var user = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        await articlesHandler.RemoveCommentAsync(slug,
            commentId,
            user!,
            new CancellationToken());
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok<CommentEnvelope<Comment>>, ValidationProblem>> CreateComment(string slug,
        CommentEnvelope<CommentDto> request,
        IArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal)
    {
        if (!MiniValidator.TryValidate(request,
                out var errors))
            return TypedResults.ValidationProblem(errors);

        var user = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await articlesHandler.AddCommentAsync(slug,
            user!,
            request.Comment,
            new CancellationToken());
        var comment = CommentMapper.MapFromCommentEntity(result);
        return TypedResults.Ok(new CommentEnvelope<Comment>(comment));
    }

    private static async Task<Ok<ArticlesResponse>> GetFeed([AsParameters] FeedQuery query,
        IArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal)
    {
        var user = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        var articlesQuery = new ArticlesQuery(null,
            null,
            null,
            query.Limit,
            query.Offset);
        var response = await articlesHandler.GetArticlesAsync(articlesQuery,
            user,
            false,
            new CancellationToken());
        var result = ArticlesMapper.MapFromArticles(response);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<ArticleEnvelope<ArticleResponse>>> UnfavoriteBySlug(string slug,
        IArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal)
    {
        var user = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        var article = await articlesHandler.DeleteFavorite(slug,
            user!,
            new CancellationToken());
        var result = ArticlesMapper.MapFromArticleEntity(article);
        return TypedResults.Ok(new ArticleEnvelope<ArticleResponse>(result));
    }

    private static async Task<Ok<ArticleEnvelope<ArticleResponse>>> FavoriteBySlug(string slug,
        IArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal)
    {
        var user = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        var article = await articlesHandler.AddFavoriteAsync(slug,
            user!,
            new CancellationToken());
        var result = ArticlesMapper.MapFromArticleEntity(article);
        return TypedResults.Ok(new ArticleEnvelope<ArticleResponse>(result));
    }

    private static async Task<Ok> DeleteArticle(string slug,
        IArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal)
    {
        var user = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        await articlesHandler.DeleteArticleAsync(slug,
            user!,
            new CancellationToken());
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok<ArticleEnvelope<ArticleResponse>>, ValidationProblem>> UpdateArticle(
        string slug,
        ArticleEnvelope<ArticleUpdateDto> request,
        IArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal)
    {
        if (!MiniValidator.TryValidate(request,
                out var errors))
            return TypedResults.ValidationProblem(errors);
        var article = await articlesHandler.UpdateArticleAsync(request.Article,
            slug,
            claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)!,
            new CancellationToken());
        var result = ArticlesMapper.MapFromArticleEntity(article);
        return TypedResults.Ok(new ArticleEnvelope<ArticleResponse>(result));
    }

    private static async Task<Results<Ok<ArticleEnvelope<ArticleResponse>>, ValidationProblem>> CreateArticle(
        ArticleEnvelope<NewArticleDto> request,
        IArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal)
    {
        if (!MiniValidator.TryValidate(request,
                out var errors))
            return TypedResults.ValidationProblem(errors);

        var user = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        var article = await articlesHandler.CreateArticleAsync(request.Article,
            user!,
            new CancellationToken());
        var result = ArticlesMapper.MapFromArticleEntity(article);
        return TypedResults.Ok(new ArticleEnvelope<ArticleResponse>(result));
    }

    private static async Task<Ok<CommentsEnvelope<List<Comment>>>> GetComments(string slug,
        IArticlesHandler articlesHandler,
        ClaimsPrincipal claimsPrincipal)
    {
        var user = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await articlesHandler.GetCommentsAsync(slug,
            user,
            new CancellationToken());
        var comments = result.Select(CommentMapper.MapFromCommentEntity);
        return TypedResults.Ok(new CommentsEnvelope<List<Comment>>(comments.ToList()));
    }
}
