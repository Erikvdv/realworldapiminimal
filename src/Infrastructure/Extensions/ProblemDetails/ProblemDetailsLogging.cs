using Hellang.Middleware.ProblemDetails;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Realworlddotnet.Infrastructure.Extensions.ProblemDetails;

public class ProblemDetailsLogging(ILogger<ProblemDetailsLogging> logger) : IPostConfigureOptions<ProblemDetailsOptions>
{
    public void PostConfigure(string? name, ProblemDetailsOptions options)
    {
        options.OnBeforeWriteDetails += (_, problem) => { logger.LogInformation("{@Problem}", problem); };
    }
}
