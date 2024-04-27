using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BuildingBlocks.Web;

public interface ICurrentUserProvider
{
    long? GetCurrentUserId();
}

public class CurrentUserProvider : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }


    public long? GetCurrentUserId()
    {
        var nameIdentifier = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        long.TryParse(nameIdentifier, out var userId);

        return userId;
    }
}
