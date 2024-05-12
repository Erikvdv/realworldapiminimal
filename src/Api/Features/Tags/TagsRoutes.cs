namespace Realworlddotnet.Api.Features.Tags;

public class TagsRoutes : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/tags", GetTags).IncludeInOpenApi();
    }

    private static async Task<TagsEnvelope<string[]>> GetTags(ITagsHandler articlesHandler, CancellationToken cancellationToken)
    {
        var tags = await articlesHandler.GetTagsAsync(cancellationToken);
        return new TagsEnvelope<string[]>(tags);
    }
}
