using System.Security.Claims;
using Data.Queries;
using TourmalineCore.AspNetCore.JwtAuthentication.Core.Contract;

namespace TourmalineCore.Authentication.Service.Services.Users
{
    public class UserClaimsProvider : IUserClaimsProvider
    {
        private readonly IUserQuery _userQuery;
        private readonly ILogger<UserClaimsProvider> _logger;

        public const string PermissionsClaimType = "permissions";

        private const string NameIdentifireClaimType = "nameIdentifier";
        private const string IdClaimType = "id";

        public UserClaimsProvider(
            IUserQuery userQuery,
            ILogger<UserClaimsProvider> logger)
        {
            _userQuery = userQuery;
            _logger = logger;
        }

        public async Task<List<Claim>> GetUserClaimsAsync(string login)
        {
            var user = await _userQuery.GetUserByUserNameAsync(login);

            if (user == null)
            {
                _logger.LogError($"[{nameof(UserClaimsProvider)}]: Could not find a user with login [{login}]");

                throw new NullReferenceException();
            }

            return await BuildClaimsAsync(login, user.Id);
        }

        private async Task<List<Claim>> BuildClaimsAsync(string login, long userId)
        {
            var claims = new List<Claim>
            {
                new Claim(NameIdentifireClaimType, login),
                new Claim(IdClaimType, userId.ToString()),
            };

            return claims;
        }
    }
}