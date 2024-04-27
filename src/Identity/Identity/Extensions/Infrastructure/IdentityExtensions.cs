using BuildingBlocks.Jwt;
using Identity.Data;
using Identity.Identity.Model;
using Identity.Identity.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Extensions.Infrastructure;

public static class IdentityServerExtensions
{
    public static WebApplicationBuilder AddCustomIdentityServer(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddScoped<ITokenService,TokenService>();   
        builder.Services.Configure<JWTSettings>(configuration.GetSection("JWTSettings"));
        builder.Services.AddIdentity<User, Role>(config =>
        {
            config.Password.RequiredLength = 6;
            config.Password.RequireDigit = false;
            config.Password.RequireNonAlphanumeric = false;
            config.Password.RequireUppercase = false;
        })
            .AddEntityFrameworkStores<IdentityContext>()
            .AddDefaultTokenProviders()
            .AddTokenProvider("Identity", typeof(DataProtectorTokenProvider<User>)); ;
        return builder;
    }
}
