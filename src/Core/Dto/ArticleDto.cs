using System.ComponentModel.DataAnnotations;
using Realworlddotnet.Core.Entities;

namespace Realworlddotnet.Core.Dto;

public record ArticlesQueryDto(string? Tag, string? Author, string? Favorited, int Limit = 20, int Offset = 0);

public record ArticlesResponseDto(List<Article> Articles, int ArticlesCount);

public record ArticleUpdateDto(string? Title, string? Description, string? Body)
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Title) && string.IsNullOrWhiteSpace(Description) &&
            string.IsNullOrWhiteSpace(Body))
        {
            yield return new ValidationResult(
                $"At least on of the fields: {nameof(Title)}, {nameof(Description)}, {nameof(Body)} must be filled"
            );
        }
    }
}
