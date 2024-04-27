using Microsoft.AspNetCore.Identity;

namespace Identity.Identity.Model;

public class User :IdentityUser<Guid>
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
}
