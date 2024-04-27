using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
namespace Identity.Data.Configurations;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<IdentityContext>
{
    public IdentityContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<IdentityContext>();

        builder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=bs_i;Trusted_Connection=True;MultipleActiveResultSets=true")
            .UseSnakeCaseNamingConvention();
        return new IdentityContext(builder.Options);
    }
}
