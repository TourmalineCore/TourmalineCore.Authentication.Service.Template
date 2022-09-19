using System.Threading.Tasks;
using Data.Models;

namespace Data.Queries
{
    public interface IUserQuery
    {
        Task<User?> GetUserByUserNameAsync(string login);
    }
}