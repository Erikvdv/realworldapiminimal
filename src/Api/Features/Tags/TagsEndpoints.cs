namespace Realworlddotnet.Api.Features.Tags;

public static class TagsEndpoints
{
    public static void AddTagsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("tags").WithTags("Tags");

        group.MapGet("/", GetTags);
    }

    private static async Task<TagsEnvelope<string[]>> GetTags(ITagsHandler articlesHandler,
        CancellationToken cancellationToken)
    {
        var tags = await articlesHandler.GetTagsAsync(cancellationToken);
        return new TagsEnvelope<string[]>(tags);
    }
}
