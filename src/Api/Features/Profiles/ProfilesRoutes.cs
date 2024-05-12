using Realworlddotnet.Core.Dto;

namespace Realworlddotnet.Api.Features.Profiles;

public class ProfilesRoutes : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("profiles")
            .RequireAuthorization()
            .WithTags("Profile")
            .IncludeInOpenApi();

        group.MapGet("{username}", GetProfile);
        group.MapPost("{followUsername}/follow", FollowProfile);
        group.MapDelete("{followUsername}/follow", UnfollowProfile);
    }

    private static async Task<Ok<ProfilesEnvelope<ProfileDto>>> GetProfile(
        string username, 
        IProfilesHandler profilesHandler, 
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var user = claimsPrincipal.GetUsername();
        var result = await profilesHandler.GetAsync(username, user, cancellationToken);
        return TypedResults.Ok(new ProfilesEnvelope<ProfileDto>(result));
    }
    
    private static async Task<Ok<ProfilesEnvelope<ProfileDto>>> FollowProfile(
        string followUsername, 
        IProfilesHandler profilesHandler, 
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var user = claimsPrincipal.GetUsername();
        var result = await profilesHandler.FollowProfileAsync(followUsername, user, cancellationToken);
        return TypedResults.Ok(new ProfilesEnvelope<ProfileDto>(result));
    }
    
    private static async Task<Ok<ProfilesEnvelope<ProfileDto>>> UnfollowProfile(
        string followUsername, 
        IProfilesHandler profilesHandler, 
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var user = claimsPrincipal.GetUsername();
        var result = await profilesHandler.UnFollowProfileAsync(followUsername, user, cancellationToken);
        return TypedResults.Ok(new ProfilesEnvelope<ProfileDto>(result));
    }
    
}
