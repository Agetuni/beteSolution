using BuildingBlocks.Jwt;
using BuildingBlocks.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Text;

namespace Identity.Extensions.Infrastructure;

public static class JwtExtensions
{
    public static IServiceCollection AddJwt(this IServiceCollection services)
    {
        var jwtOptions = services.GetOptions<JWTSettings>("JWTSettings");
        Console.WriteLine(jwtOptions.Key);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
           .AddJwtBearer(o =>
           {
               o.RequireHttpsMetadata = false;
               o.SaveToken = false;
               o.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuerSigningKey = true,
                   ValidateIssuer = true,
                   ValidateAudience = true,
                   ValidateLifetime = true,
                   ClockSkew = TimeSpan.Zero,
                   ValidIssuer =jwtOptions.Issuer,
                   ValidAudience = jwtOptions.Audience,
                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
               };
               o.Events = new JwtBearerEvents()
               {
                   OnAuthenticationFailed = c =>
                   {
                       c.NoResult();
                       c.Response.StatusCode = 500;
                       c.Response.ContentType = "text/plain";
                       return c.Response.WriteAsync(c.Exception.ToString());
                   },
                   OnChallenge = context =>
                   {
                       context.HandleResponse();
                       context.Response.StatusCode = 401;
                       context.Response.ContentType = "application/json";
                       var result = JsonConvert.SerializeObject(new { value = "You are not Authorized" });
                       return context.Response.WriteAsync(result);
                   },
                   OnForbidden = context =>
                   {
                       context.Response.StatusCode = 403;
                       context.Response.ContentType = "application/json";
                       var result = JsonConvert.SerializeObject(new { value = "You are not authorized to access this resource" });
                       return context.Response.WriteAsync(result);
                   },
               };
           });

        services.AddAuthorization();
        return services;
    }

}

