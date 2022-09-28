using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using TourmalineCore.AspNetCore.JwtAuthentication.Core.Models;
using TourmalineCore.AspNetCore.JwtAuthentication.Core.Models.Response;
using Xunit;

namespace Tests.Integration;

public class RegistrationAndLoginTests : TestBase
{
    private const string Login = "test_login";
    private const string Password = "testtesTtest!1";

    public RegistrationAndLoginTests(WebApplicationFactory<Program> factory)
        : base(factory)
    {
    }

    [Fact]
    public async void Registration_WithInvalidCreds()
    {
        var registrationResponse = await RegistrationAsync(Login, "1");

        Assert.Equal(HttpStatusCode.Conflict, registrationResponse.response.StatusCode);
    }

    [Fact]
    public async void Login_WithInvalidCreds()
    {
        var registrationResponse = await LoginAsync("invalid", "invalid");

        Assert.Equal(HttpStatusCode.Unauthorized, registrationResponse.response.StatusCode);
    }

    [Fact]
    public async void Registration_ThenLogin_ReturnsTokens()
    {
        var registrationResponse = await RegistrationAsync(Login, Password);
        var (_, authModel) = await LoginAsync(Login, Password);

        Assert.Equal(HttpStatusCode.OK, registrationResponse.response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(authModel.AccessToken.Value));
        Assert.False(string.IsNullOrWhiteSpace(authModel.RefreshToken.Value));
    }

    [Fact]
    public async void Login_ThenRefreshToken_ReturnsNewTokens()
    {
        var (_, authModel) = await LoginAsync("admin", "admin");

        var (_, authModelWithNewTokens) = await CallRefreshAsync(authModel);

        Assert.False(string.IsNullOrWhiteSpace(authModelWithNewTokens.AccessToken.Value));
        Assert.False(string.IsNullOrWhiteSpace(authModelWithNewTokens.RefreshToken.Value));
    }

    [Fact]
    public async Task RefreshWithInvalidToken_Returns401()
    {
        var invalidAuthResponseModel = new AuthResponseModel
        {
            RefreshToken = new TokenModel
            {
                Value = Guid.NewGuid().ToString()
            },
            AccessToken = new TokenModel
            {
                Value = string.Empty
            }
        };

        var (response, _) = await CallRefreshAsync(invalidAuthResponseModel);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task LogoutWithValidToken_Return200()
    {
        var loginResult = await LoginAsync("admin", "admin");

        var logoutStatusCode = await LogoutAsync(loginResult.authModel);

        Assert.Equal(HttpStatusCode.OK, logoutStatusCode);
    }
}