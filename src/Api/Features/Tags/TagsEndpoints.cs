namespace Realworlddotnet.Api.Features.Tags;

public static class TagsEndpoints
{
    public static void AddTagsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("tags").WithTags("Tags");

        group.MapGet("/", GetTags);
    }

    private static async Task<TagsEnvelope<string[]>> GetTags(TagsHandler tagsHandler,
        CancellationToken cancellationToken)
    {
        var tags = await tagsHandler.GetTagsAsync(cancellationToken);
        return new TagsEnvelope<string[]>(tags);
    }
}
