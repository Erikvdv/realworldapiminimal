using Realworlddotnet.Core.Dto;
using Realworlddotnet.Core.Repositories;

namespace Realworlddotnet.Api.Features.Profiles;

public class ProfilesHandler(IConduitRepository repository) : IProfilesHandler
{
    public async Task<ProfileDto> GetAsync(string profileUsername, string? username,
        CancellationToken cancellationToken)
    {
        var profileUser = await repository.GetUserByUsernameAsync(profileUsername, cancellationToken);

        if (profileUser is null)
        {
            throw new ProblemDetailsException(422, "Profile not found");
        }

        var isFollowing = false;

        if (!string.IsNullOrEmpty(username))
        {
            isFollowing = await repository.IsFollowingAsync(profileUsername, username, cancellationToken);
        }

        return new ProfileDto(profileUser.Username, profileUser.Bio, profileUser.Image, isFollowing);
    }

    public async Task<ProfileDto> FollowProfileAsync(string profileUsername, string username,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(username))
            throw new ProblemDetailsException(422, "Not logged in");
        
        var profileUser = await repository.GetUserByUsernameAsync(profileUsername, cancellationToken);

        if (profileUser is null)
            throw new ProblemDetailsException(422, "Profile not found");
        

        repository.Follow(profileUsername, username);
        await repository.SaveChangesAsync(cancellationToken);

        return new ProfileDto(profileUser.Username, profileUser.Bio, profileUser.Email, true);
    }

    public async Task<ProfileDto> UnFollowProfileAsync(string profileUsername, string username,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(username))
            throw new ProblemDetailsException(422, "Not logged in");
        
        var profileUser = await repository.GetUserByUsernameAsync(profileUsername, cancellationToken);

        if (profileUser is null)
        {
            throw new ProblemDetailsException(422, "Profile not found");
        }

        repository.UnFollow(profileUsername, username);
        await repository.SaveChangesAsync(cancellationToken);

        return new ProfileDto(profileUser.Username, profileUser.Bio, profileUser.Email, false);
    }
}
