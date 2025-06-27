using pion_api.Models;

namespace pion_api.Services;

public interface IJwtService
{
    public string GenerateToken(ApplicationUser user);
}