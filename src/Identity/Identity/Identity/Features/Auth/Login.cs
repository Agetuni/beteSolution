using BuildingBlocks;
using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web;
using FluentValidation;
using Identity.Identity.Model;
using Identity.Identity.Service;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace Identity.Identity.Features.Auth;

//dto
public record LoginResponseDto(Guid Id, string Name, string UserName, List<string> Roles, string AccessToken, string RefreshToken);

//command
public record LoginRequest(string UserName, string Password) : ICommand<LoginResponseDto>;

//command validator
public class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().WithMessage("please enter username");
        RuleFor(x => x.Password).NotEmpty().WithMessage("please enter password");
    }
}

//command Hanlder
internal class LoginHandler : ICommandHandler<LoginRequest, LoginResponseDto>
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly ITokenService _tokenService;
    public LoginHandler(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        SignInManager<User> signInManager,
        ITokenService tokenService,
    IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    public async Task<LoginResponseDto> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user is null) throw new BadRequestException("Invalid username or password.");

        var signInResult = await _signInManager.PasswordSignInAsync(user, request.Password, false, lockoutOnFailure: false);
        if (!signInResult.Succeeded) throw new BadRequestException("Invalid username or password.");

        var userRoles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
        var accesstoken = new JwtSecurityTokenHandler().WriteToken(await _tokenService.GenerateJWToken(user, userRoles.ToList()));
        var refershToken = await _tokenService.GenerateRefreshToken(user);

        return new LoginResponseDto(user.Id, $"{user.FirstName} {user.LastName}", user.UserName, userRoles.ToList(), accesstoken, refershToken);
    }


}
public class Login : IMinimalEndpoint
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost($"{EndpointConfig.BaseApiPath}/identity/login", async (
                LoginRequest request, IMediator mediator, IMapper mapper,
                CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(request, cancellationToken);
            return Results.Ok(result);
        })
            .RequireAuthorization()
            .WithName("Login")
            .WithApiVersionSet(builder.NewApiVersionSet("auth").Build())
            .Produces<LoginResponseDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("user login")
            .WithDescription("user login")
            .WithOpenApi()
            .HasApiVersion(1.0);

        return builder;
    }
}
