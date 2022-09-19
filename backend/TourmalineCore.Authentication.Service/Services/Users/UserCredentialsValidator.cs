using Data;
using Data.Queries;
using TourmalineCore.AspNetCore.JwtAuthentication.Core.Contract;

namespace TourmalineCore.Authentication.Service.Services.Users
{
    public class UserCredentialsValidator : IUserCredentialsValidator
    {
        private readonly AppDbContext _appDbContext;


        private readonly ILogger<UserCredentialsValidator> _logger;
        private readonly IUserQuery _userQuery;

        public UserCredentialsValidator(
            AppDbContext appDbContext,
            ILogger<UserCredentialsValidator> logger,
            IUserQuery userQuery)
        {
            _appDbContext = appDbContext;
            _logger = logger;
            _userQuery = userQuery;
        }

        public async Task<bool> ValidateUserCredentials(string username, string password)
        {
            var user = await _userQuery.GetUserByUserNameAsync(username);

            if (user == null)
            {
                _logger.LogWarning($"[{nameof(UserCredentialsValidator)}]: User with credentials [{username}] not found.");
                return false;
            }

            await _appDbContext.SaveChangesAsync();

            if (user.IsBlocked)
            {
                _logger.LogWarning(
                    $"[{nameof(UserCredentialsValidator)}]: User with credentials [{username}] was blocked.");
                return false;
            }

            await _appDbContext.SaveChangesAsync();
            return true;
        }
    }
}