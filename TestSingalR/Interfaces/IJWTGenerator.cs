using TestSingalR.Entity;

namespace TestSingalR.Interfaces
{
    public interface IJwtGenerator
    {
        string CreateToken(AppUser user);
    }
}
