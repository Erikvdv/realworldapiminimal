using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Models;

namespace Realworlddotnet.Infrastructure.Extensions.OpenApi;

public static class OpenApiExtensions
{
    public static RouteGroupBuilder WithUnauthenticated(this RouteGroupBuilder builder)
    {
        builder.WithOpenApi(x =>
        {
            var response = new OpenApiResponse { Description = "Unauthenticated" };
            x.Responses.Add("401", response);
            return x;
        });

        return builder;
    }
}
