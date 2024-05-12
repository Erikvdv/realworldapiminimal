using Realworlddotnet.Core.Repositories;

namespace Realworlddotnet.Api.Features.Tags;

public class TagsHandler(IConduitRepository repository) : ITagsHandler
{
    public async Task<string[]> GetTagsAsync(CancellationToken cancellationToken)
    {
        var tags = await repository.GetTagsAsync(cancellationToken);
        return tags.Select(x => x.Id).ToArray();
    }
}
