using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using TourmalineCore.AspNetCore.JwtAuthentication.Core.Models.Request;
using TourmalineCore.AspNetCore.JwtAuthentication.Core.Models.Response;
using Xunit;

namespace Tests.Integration;

public class TestBase : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly JsonSerializerOptions _jsonSerializerSettings;

    private const string LoginUrl = "/auth/login";
    private const string RegisterUrl = "/auth/register";
    private const string RefreshUrl = "/auth/refresh";
    private const string LogoutUrl = "/auth/logout";

    public TestBase(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Tests");

        _factory = factory;

        _jsonSerializerSettings = new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            AllowTrailingCommas = true,
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };
    }

    internal async Task<(HttpResponseMessage response, AuthResponseModel authModel)> RegistrationAsync(string login,
        string password)
    {
        var client = _factory.CreateClient();

        var body = JsonContent.Create(new RegistrationRequestModel
            {
                Login = login,
                Password = password
            }
        );

        var response = await client.PostAsync(RegisterUrl, body);
        var result =
            JsonSerializer.Deserialize<AuthResponseModel>(response.Content.ReadAsStringAsync().Result,
                _jsonSerializerSettings);
        return (response, result);
    }

    internal async Task<(HttpResponseMessage response, AuthResponseModel authModel)> LoginAsync(string login,
        string password)
    {
        var client = _factory.CreateClient();

        var body = JsonContent.Create(new LoginRequestModel
            {
                Login = login,
                Password = password
            }
        );

        var response = await client.PostAsync(LoginUrl, body);

        var authModel =
            JsonSerializer.Deserialize<AuthResponseModel>(response.Content.ReadAsStringAsync().Result,
                _jsonSerializerSettings);

        return (response, authModel);
    }

    internal async Task<(HttpResponseMessage response, AuthResponseModel authModel)> CallRefreshAsync(
        AuthResponseModel authResponseModel)
    {
        var client = _factory.CreateClient();

        var body = JsonContent.Create(new RefreshTokenRequestModel
            {
                RefreshTokenValue = Guid.Parse(authResponseModel.RefreshToken.Value)
            }
        );

        var response = await client.PostAsync(RefreshUrl, body);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponseModel.AccessToken.Value);
        var result =
            JsonSerializer.Deserialize<AuthResponseModel>(await response.Content.ReadAsStringAsync(),
                _jsonSerializerSettings);
        return (response, result);
    }

    internal async Task<HttpStatusCode> LogoutAsync(AuthResponseModel authResponseModel)
    {
        var client = _factory.CreateClient();

        var body = JsonContent.Create(new RefreshTokenRequestModel
            {
                RefreshTokenValue = Guid.Parse(authResponseModel.RefreshToken.Value)
            }
        );

        var response = await client.PostAsync(LogoutUrl, body);
        return response.StatusCode;
    }
}