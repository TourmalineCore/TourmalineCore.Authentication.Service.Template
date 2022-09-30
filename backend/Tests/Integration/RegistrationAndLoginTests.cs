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
    public async Task Registration_WithInvalidCreds()
    {
        var registrationResponse = await RegistrationAsync(Login, "1");

        Assert.NotEqual(HttpStatusCode.OK, registrationResponse.response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidCreds()
    {
        var registrationResponse = await LoginAsync("invalid", "invalid");

        Assert.Equal(HttpStatusCode.Unauthorized, registrationResponse.response.StatusCode);
    }

    [Fact]
    public async Task Registration_ThenLoginForCreatedUser_ReturnsTokens()
    {
        var registrationResponse = await RegistrationAsync(Login, Password);
        var (_, authModel) = await LoginAsync(Login, Password);

        Assert.Equal(HttpStatusCode.OK, registrationResponse.response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(authModel.AccessToken.Value));
        Assert.False(string.IsNullOrWhiteSpace(authModel.RefreshToken.Value));
    }
}