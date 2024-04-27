using BuildingBlocks;
using BuildingBlocks.Jwt;
using Identity.Identity.Model;
using MassTransit.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.Identity.Service;

public interface ITokenService
{
    Task<JwtSecurityToken> GenerateJWToken(User user, List<string> roles, string ipAddress = "0.0.0.");
    Task<string> GenerateRefreshToken(User user);
}
public class TokenService : ITokenService
{
    private readonly UserManager<User> _userManager;
    private readonly JWTSettings _jwtSettings;
    public TokenService(UserManager<User> userManager, IOptions<JWTSettings> jwtSettings) => (_userManager, _jwtSettings) = (userManager, jwtSettings.Value);

    public async Task<JwtSecurityToken> GenerateJWToken(User user, List<string> roles, string ipAddress = "0.0.0.")
    {
        var roleClaims = new List<Claim>();
        for (int i = 0; i < roles.Count; i++)
            roleClaims.Add(new Claim("roles", roles[i]));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("uid", user.Id.ToString()),
            new Claim("ip", ipAddress)
        }
        .Union(roleClaims);

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        var jwtSecurityToken = new JwtSecurityToken(
        issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
            signingCredentials: signingCredentials);

        return jwtSecurityToken;
    }

    public async Task<string> GenerateRefreshToken(User user)
    {
        await _userManager.RemoveAuthenticationTokenAsync(user, "Identity", "RefreshToken");
        var newRefreshToken = await _userManager.GenerateUserTokenAsync(user, "Identity", "RefreshToken");
        IdentityResult result = await _userManager.SetAuthenticationTokenAsync(user, "Identity", "RefreshToken", newRefreshToken);
        if (!result.Succeeded) throw new InternalServerException("Error has occured. please try again");
        return newRefreshToken;
    }
}
