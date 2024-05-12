using System.ComponentModel.DataAnnotations;

namespace Realworlddotnet.Core.Entities;

public class Tag(string id)
{
    [MaxLength(30)]
    public string Id { get; set; } = id;

    public ICollection<Article> Articles { get; set; } = null!;
}
