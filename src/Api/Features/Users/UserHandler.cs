using Microsoft.AspNetCore.Mvc;
using Realworlddotnet.Core.Dto;
using Realworlddotnet.Core.Repositories;

namespace Realworlddotnet.Api.Features.Users;

public class UserHandler(IConduitRepository repository, ITokenGenerator tokenGenerator)
{
    public async Task<UserDto> CreateAsync(NewUserDto newUser, CancellationToken cancellationToken)
    {
        if (await repository.UserExistsAsync(newUser.Username))
        {
            throw new ProblemDetailsException(new ValidationProblemDetails
            {
                Status = 422,
                Detail = "Cannot register user",
                Errors = { new KeyValuePair<string, string[]>("Username", new[] { "Username not available" }) }
            });
        }

        if (await repository.EmailExistsAsync(newUser.Email))
        {
            throw new ProblemDetailsException(new ValidationProblemDetails
            {
                Status = 422,
                Detail = "Cannot register user",
                Errors = { new KeyValuePair<string, string[]>("Email", new[] { "Email address already in use" }) }
            });
        }
        var user = new User(newUser);
        repository.AddUser(user);
        await repository.SaveChangesAsync(cancellationToken);
        var token = tokenGenerator.CreateToken(user.Username);
        return new UserDto(user.Username, user.Email, token, user.Bio, user.Image);
    }

    public async Task<UserDto> UpdateAsync(
        string username, UpdatedUserDto updatedUser, CancellationToken cancellationToken)
    {
        var user = await repository.GetUserByUsernameAsync(username, cancellationToken);
        user.UpdateUser(updatedUser);
        await repository.SaveChangesAsync(cancellationToken);
        var token = tokenGenerator.CreateToken(user.Username);
        return new UserDto(user.Username, user.Email, token, user.Bio, user.Image);
    }

    public async Task<UserDto> LoginAsync(LoginUserDto login, CancellationToken cancellationToken)
    {
        var user = await repository.GetUserByEmailAsync(login.Email);

        if (user == null || user.Password != login.Password)
        {
            throw new ProblemDetailsException(422, "Incorrect Credentials");
        }

        var token = tokenGenerator.CreateToken(user.Username);
        return new UserDto(user.Username, user.Email, token, user.Bio, user.Image);
    }

    public async Task<UserDto> GetAsync(string username, CancellationToken cancellationToken)
    {
        var user = await repository.GetUserByUsernameAsync(username, cancellationToken);
        var token = tokenGenerator.CreateToken(user.Username);
        return new UserDto(user.Username, user.Email, token, user.Bio, user.Image);
    }
}
