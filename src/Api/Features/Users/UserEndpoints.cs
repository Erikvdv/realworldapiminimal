using Realworlddotnet.Core.Dto;
using Realworlddotnet.Infrastructure.Extensions.OpenApi;

namespace Realworlddotnet.Api.Features.Users;

public static class UserEndpoints
{
    public static void AddUserEndpoints(this IEndpointRouteBuilder app)
    {
        var userGroup = app.MapGroup("user").RequireAuthorization().WithTags("User")
            .WithUnauthenticated();
        var usersGroup = app.MapGroup("users").WithTags("User");

        userGroup.MapGet("/", GetUser);
        userGroup.MapPut("/", UpdateUser);

        usersGroup.MapPost("/", CreateUser);
        usersGroup.MapPost("/login", LoginUser);
    }

    private static async Task<Results<ValidationProblem, Ok<UserEnvelope<UserDto>>>> LoginUser(
        UserHandler userHandler,
        UserEnvelope<LoginUserDto> request,
        CancellationToken cancellationToken)
    {
        if (!MiniValidator.TryValidate(request, out var errors))
        {
            return TypedResults.ValidationProblem(errors);
        }
        
        var user = await userHandler.LoginAsync(request.User, cancellationToken);
        return TypedResults.Ok(new UserEnvelope<UserDto>(user));
    }

    private static async Task<IResult> CreateUser(
        UserHandler userHandler,
        UserEnvelope<NewUserDto> request,
        CancellationToken cancellationToken)
    {
        if (!MiniValidator.TryValidate(request, out var errors))
        {
            return Results.ValidationProblem(errors);
        }

        var user = await userHandler.CreateAsync(request.User, cancellationToken);
        return Results.Ok(new UserEnvelope<UserDto>(user));
    }

    private static async Task<Results<ValidationProblem, Ok<UserEnvelope<UserDto>>>> UpdateUser(
        UserHandler userHandler,
        ClaimsPrincipal claimsPrincipal,
        UserEnvelope<UpdatedUserDto> request,
        CancellationToken cancellationToken)
    {
        if (!MiniValidator.TryValidate(request, out var errors))
        {
            return TypedResults.ValidationProblem(errors);
        }

        var username = claimsPrincipal.GetUsername();
        var user = await userHandler.UpdateAsync(username, request.User, cancellationToken);
        return TypedResults.Ok(new UserEnvelope<UserDto>(user));
    }

    private static async Task<UserEnvelope<UserDto>> GetUser(
        UserHandler userHandler,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var username = claimsPrincipal.GetUsername();
        var user = await userHandler.GetAsync(username, cancellationToken);
        return new UserEnvelope<UserDto>(user);
    }
}
