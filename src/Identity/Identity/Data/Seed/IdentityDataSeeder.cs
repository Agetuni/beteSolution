using BuildingBlocks.EFCore;
using Identity.Identity.Model;
using Microsoft.AspNetCore.Identity;

namespace Identity.Data.Seed;

public class IdentityDataSeeder : IDataSeeder
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IdentityContext _identityContext;

    public IdentityDataSeeder(UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IdentityContext identityContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _identityContext = identityContext;
    }

    public async Task SeedAllAsync()
    {
        await SeedTenant();
        await SeedRoles();
        await SeedUsers();
    }
    private async Task SeedTenant()
    {
        _identityContext.Database.EnsureCreated();
        await _identityContext.Tenant.AddAsync(InitialData.Tenant);
        await _identityContext.SaveChangesAsync();
    }
    private async Task SeedRoles()
    {
        if (await _roleManager.RoleExistsAsync("super_admin") == false)
        {
            await _roleManager.CreateAsync(new Role { Name = "super_admin" });
        }

        if (await _roleManager.RoleExistsAsync("admin") == false)
        {
            await _roleManager.CreateAsync(new Role { Name = "admin" });
        }
        if (await _roleManager.RoleExistsAsync("user") == false)
        {
            await _roleManager.CreateAsync(new Role { Name = "user" });
        }
    }
    private async Task SeedUsers()
    {
        _identityContext.Database.EnsureCreated();
        var tenant = _identityContext.Tenant.First();
        if (await _userManager.FindByNameAsync("Aleazar") == null)
        {
            var result = await _userManager.CreateAsync(InitialData.Users.First(), "User@123456");

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(InitialData.Users.First(), "super_admin");
                _identityContext.UserTenant.Add(new UserTenant { Tenant = tenant, User = InitialData.Users.First() });

            }
        }

        if (await _userManager.FindByNameAsync("Raymond") == null)
        {
            var result = await _userManager.CreateAsync(InitialData.Users.Last(), "User@123456");

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(InitialData.Users.Last(), "user");
                _identityContext.UserTenant.Add(new UserTenant { Tenant = tenant, User = InitialData.Users.Last() });

            }
        }
    }
}