namespace Realworlddotnet.Api.Features.Tags;

public class TagsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/tags",
                GetTags)
            .Produces<TagsEnvelope<string[]>>()
            .WithTags("Tags")
            .WithName("GetTags")
            .IncludeInOpenApi();
    }

    private static async Task<TagsEnvelope<string[]>> GetTags(ITagsHandler articlesHandler)
    {
        var tags = await articlesHandler.GetTagsAsync(new CancellationToken());
        return new TagsEnvelope<string[]>(tags);
    }
}
