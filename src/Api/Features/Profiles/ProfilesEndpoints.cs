using Realworlddotnet.Core.Dto;
using Realworlddotnet.Infrastructure.Extensions.OpenApi;

namespace Realworlddotnet.Api.Features.Profiles;

public static class ProfilesEndpoints
{
    public static void AddProfilesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("profiles").RequireAuthorization().WithTags("Profile").WithUnauthenticated();

        group.MapGet("{username}", GetProfile);
        group.MapPost("{followUsername}/follow", FollowProfile);
        group.MapDelete("{followUsername}/follow", UnfollowProfile);
    }

    private static async Task<Ok<ProfilesEnvelope<ProfileDto>>> GetProfile(
        string username,
        ProfilesHandler profilesHandler,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var user = claimsPrincipal.GetUsername();
        var result = await profilesHandler.GetAsync(username, user, cancellationToken);
        return TypedResults.Ok(new ProfilesEnvelope<ProfileDto>(result));
    }

    private static async Task<Ok<ProfilesEnvelope<ProfileDto>>> FollowProfile(
        string followUsername,
        ProfilesHandler profilesHandler,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var user = claimsPrincipal.GetUsername();
        var result = await profilesHandler.FollowProfileAsync(followUsername, user, cancellationToken);
        return TypedResults.Ok(new ProfilesEnvelope<ProfileDto>(result));
    }

    private static async Task<Ok<ProfilesEnvelope<ProfileDto>>> UnfollowProfile(
        string followUsername,
        ProfilesHandler profilesHandler,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var user = claimsPrincipal.GetUsername();
        var result = await profilesHandler.UnFollowProfileAsync(followUsername, user, cancellationToken);
        return TypedResults.Ok(new ProfilesEnvelope<ProfileDto>(result));
    }
}
