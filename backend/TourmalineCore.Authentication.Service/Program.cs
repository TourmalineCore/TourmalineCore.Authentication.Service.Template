using Data;
using Data.Models;
using Data.Queries;
using Microsoft.EntityFrameworkCore;
using TourmalineCore.AspNetCore.JwtAuthentication.Core;
using TourmalineCore.AspNetCore.JwtAuthentication.Core.Models.Request;
using TourmalineCore.AspNetCore.JwtAuthentication.Core.Options;
using TourmalineCore.AspNetCore.JwtAuthentication.Identity;
using TourmalineCore.AspNetCore.JwtAuthentication.Identity.Options;
using TourmalineCore.Authentication.Service.Services.Callbacks;
using TourmalineCore.Authentication.Service.Services.Users;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
const string defaultConnection = "DefaultConnection";

builder.Services.AddControllers();
builder.Services.AddCors();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Tests")
    builder
        .Services.AddDbContext<AppDbContext>(options =>
            {
                AppDbContext.ConfigureContextOptions(options, configuration.GetConnectionString(defaultConnection));
            }
        );

var authenticationOptions = configuration.GetSection(nameof(AuthenticationOptions)).Get<RefreshAuthenticationOptions>();

builder.Services
    .AddJwtAuthenticationWithIdentity<AppDbContext, User, long>()
    .AddLoginWithRefresh(authenticationOptions)
    .AddRefreshConfidenceInterval()
    .AddLogout()
    .AddUserCredentialsValidator<UserCredentialsValidator>()
    .WithUserClaimsProvider<UserClaimsProvider>(UserClaimsProvider.PermissionsClaimType)
    .AddRegistration();


builder.Services.AddSingleton<AuthCallbacks>();
builder.Services.AddTransient<IUserQuery, UserQuery>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsEnvironment("Debug"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using var serviceScope = app.Services.CreateScope();

app
    .OnLoginExecuting(serviceScope.ServiceProvider.GetRequiredService<AuthCallbacks>().OnLoginExecuting)
    .OnLoginExecuted(serviceScope.ServiceProvider.GetRequiredService<AuthCallbacks>().OnLoginExecuted)
    .UseDefaultLoginMiddleware(new LoginEndpointOptions
        {
            LoginEndpointRoute = "/auth/login"
        }
    )
    .UseRefreshTokenMiddleware(new RefreshEndpointOptions
        {
            RefreshEndpointRoute = "/auth/refresh"
        }
    )
    .UseRefreshTokenLogoutMiddleware(new LogoutEndpointOptions
        {
            LogoutEndpointRoute = "/auth/logout"
        }
    )
    .UseRegistration<User, long, RegistrationRequestModel>(requestModel => new User
        {
            UserName = requestModel.Login,
            PasswordHash = requestModel.Password
        },
        new RegistrationEndpointOptions
        {
            RegistrationEndpointRoute = "/auth/register"
        });

if (!app.Environment.IsEnvironment("Tests"))
{
    var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}
else
{
    app.UseDefaultDbUser<AppDbContext, User, long>("admin", "admin");
}

app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.Run();

public partial class Program
{
}