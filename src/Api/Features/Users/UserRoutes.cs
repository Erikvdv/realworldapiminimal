using Realworlddotnet.Core.Dto;

namespace Realworlddotnet.Api.Features.Users;

public class UserRoutes : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var userGroup = app.MapGroup("user")
            .RequireAuthorization()
            .WithTags("User")
            .IncludeInOpenApi();
        
        var usersGroup = app.MapGroup("users")
            .RequireAuthorization()
            .WithTags("User")
            .IncludeInOpenApi();
        
        userGroup.MapGet("/user", GetUser)
            .Produces<UserEnvelope<UserDto>>()
            .WithName("GetUser");

        userGroup.MapPut("/user", UpdateUser)
            .Produces<UserEnvelope<UserDto>>()
            .WithName("UpdateUser");

        usersGroup.MapPost("/users",
                CreateUser)
            .Produces<UserEnvelope<UserDto>>()
            .WithName("CreateUser");

        usersGroup.MapPost("/users/login",
                LoginUser)
            .Produces<UnprocessableEntity<ValidationProblem>>(422)
            .WithName("LoginUser")
            .ProducesValidationProblem();
    }

    private static async Task<Results<ValidationProblem, Ok<UserEnvelope<UserDto>>>> LoginUser(IUserHandler userHandler,
        UserEnvelope<LoginUserDto> request)
    {
        if (!MiniValidator.TryValidate(request, out var errors))
        {
            return TypedResults.ValidationProblem(errors);
        }

        var user = await userHandler.LoginAsync(request.User, new CancellationToken());
        return TypedResults.Ok(new UserEnvelope<UserDto>(user));
    }

    private static async Task<IResult> CreateUser(IUserHandler userHandler, UserEnvelope<NewUserDto> request)
    {
        if (!MiniValidator.TryValidate(request, out var errors))
        {
            return Results.ValidationProblem(errors);
        }

        var user = await userHandler.CreateAsync(request.User, new CancellationToken());
        return Results.Ok(new UserEnvelope<UserDto>(user));
    }

    private static async Task<IResult> UpdateUser(IUserHandler userHandler, ClaimsPrincipal claimsPrincipal,
        UserEnvelope<UpdatedUserDto> request)
    {
        if (!MiniValidator.TryValidate(request, out var errors)) return Results.ValidationProblem(errors);

        var username = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await userHandler.UpdateAsync(username!, request.User, new CancellationToken());
        return Results.Ok(new UserEnvelope<UserDto>(user));
    }
    
    private static async Task<UserEnvelope<UserDto>> GetUser(IUserHandler userHandler, ClaimsPrincipal claimsPrincipal)
    {
        var username = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await userHandler.GetAsync(username!, new CancellationToken());
        return new UserEnvelope<UserDto>(user);
    }
}
