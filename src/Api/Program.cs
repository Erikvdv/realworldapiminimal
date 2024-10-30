using System.Configuration;
using System.Security.Authentication;
using Realworlddotnet.Api.Features.Articles;
using Realworlddotnet.Api.Features.Profiles;
using Realworlddotnet.Api.Features.Tags;
using Realworlddotnet.Api.Features.Users;
using Realworlddotnet.Core.Repositories;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// add logging
builder.Host.UseSerilog((hostBuilderContext, services, loggerConfiguration) =>
{
    loggerConfiguration.ConfigureBaseLogging("realworldDotnet");
});

// setup database connection (used for in memory SQLite).
// SQLite in memory requires an open connection during the application lifetime
#pragma warning disable S125
// to use a file based SQLite use: "Filename=../realworld.db";
#pragma warning restore S125
const string connectionString = "Filename=:memory:";
var connection = new SqliteConnection(connectionString);
await connection.OpenAsync();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails(o =>
    {
        o.CustomizeProblemDetails = context =>
        { 
            context.ProblemDetails.Title = context.Exception?.Message ?? context.ProblemDetails.Title;
        };
    });

builder.Services.AddScoped<IConduitRepository, ConduitRepository>();
builder.Services.AddScoped<UserHandler>();
builder.Services.AddScoped<ArticlesHandler>();
builder.Services.AddScoped<TagsHandler>();
builder.Services.AddScoped<ProfilesHandler>();
builder.Services.AddSingleton<CertificateProvider>();

builder.Services.AddSingleton<ITokenGenerator>(container =>
{
    var logger = container.GetRequiredService<ILogger<CertificateProvider>>();
    var certificateProvider = new CertificateProvider(logger);
    var cert = certificateProvider.LoadFromFile("certificate.pfx", "password");

    return new TokenGenerator(cert);
});

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<ILogger<CertificateProvider>>((o, logger) =>
    {
        var certificateProvider = new CertificateProvider(logger);
        var cert = certificateProvider.LoadFromFile("certificate.pfx", "password");

        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            IssuerSigningKey = new RsaSecurityKey(cert.GetRSAPublicKey())
        };
        o.Events = new JwtBearerEvents { OnMessageReceived = CustomOnMessageReceivedHandler.OnMessageReceived };
    });

// for SQLite in memory a connection is provided rather than a connection string
builder.Services.AddDbContext<ConduitContext>(options => { options.UseSqlite(connection); });



var app = builder.Build();

// when using in memory SQLite ensure the tables are created
using (var scope = app.Services.CreateScope())
{
    await using var context = scope.ServiceProvider.GetService<ConduitContext>() ?? throw new Exception("Could not get ConduitContext");
    await context.Database.EnsureCreatedAsync();
}

app.UseSerilogRequestLogging(options =>
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        diagnosticContext.Set("UserId", httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "")
);


app.UseExceptionHandler(new ExceptionHandlerOptions
{
    StatusCodeSelector = ex => ex switch
    {
        AuthenticationException => StatusCodes.Status401Unauthorized,
        UnauthorizedAccessException => StatusCodes.Status403Forbidden,
        KeyNotFoundException => StatusCodes.Status404NotFound,
        ValidationException => StatusCodes.Status422UnprocessableEntity,
        _ => StatusCodes.Status500InternalServerError
    }
});
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();

app.AddTagsEndpoints();
app.AddProfilesEndpoints();
app.AddArticlesEndpoints();
app.AddUserEndpoints();

app.MapScalarApiReference();
app.MapOpenApi();

try
{
    Log.Information("Starting web host");
    await app.RunAsync();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    await connection.CloseAsync();
    await Log.CloseAndFlushAsync();
    Thread.Sleep(2000);
}
