using Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.Queries
{
    public class UserQuery : IUserQuery
    {
        private readonly AppDbContext _context;

        public UserQuery(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByUserNameAsync(string login)
        {
            var tt =  await GetUsersAsQueryable()
                .SingleOrDefaultAsync(x => x.UserName == login);

            return tt;
        }

        private IQueryable<User?> GetUsersAsQueryable()
        {
            return _context
                .Set<User>()
                .AsQueryable();
        }
    }
}