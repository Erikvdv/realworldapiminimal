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

        group.MapGet("{username}", GetProfile)
            .WithName("GetProfile");

        group.MapPost("{followUsername}/follow", FollowProfile)
            .WithName("FollowProfile");

        group.MapDelete("{followUsername}/follow", UnfollowProfile)
            .WithName("UnfollowProfile");
    }

    private static async Task<Ok<ProfilesEnvelope<ProfileDto>>> GetProfile(string username, IProfilesHandler profilesHandler, ClaimsPrincipal claimsPrincipal)
    {
        var user = claimsPrincipal.GetUsername();
        var result = await profilesHandler.GetAsync(username, user, new CancellationToken());
        return TypedResults.Ok(new ProfilesEnvelope<ProfileDto>(result));
    }
    
    private static async Task<Ok<ProfilesEnvelope<ProfileDto>>> FollowProfile(string followUsername, IProfilesHandler profilesHandler, ClaimsPrincipal claimsPrincipal)
    {
        var user = claimsPrincipal.GetUsername();
        var result = await profilesHandler.FollowProfileAsync(followUsername, user, new CancellationToken());
        return TypedResults.Ok(new ProfilesEnvelope<ProfileDto>(result));
    }
    
    private static async Task<Ok<ProfilesEnvelope<ProfileDto>>> UnfollowProfile(string followUsername, IProfilesHandler profilesHandler, ClaimsPrincipal claimsPrincipal)
    {
        var user = claimsPrincipal.GetUsername();
        var result = await profilesHandler.UnFollowProfileAsync(followUsername, user, new CancellationToken());
        return TypedResults.Ok(new ProfilesEnvelope<ProfileDto>(result));
    }
    
}
