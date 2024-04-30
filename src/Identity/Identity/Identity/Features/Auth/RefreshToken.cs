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
using System.IdentityModel.Tokens.Jwt;

namespace Identity.Identity.Features.Auth;

//command
public record RefreshTokenCommand(string UserName, string token) : ICommand<LoginResponseDto>;

//login validator
public class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().WithMessage("please enter username");
        RuleFor(x => x.token).NotEmpty().WithMessage("please enter password");
    }
}

// handler 
public class RefreshTokenHandler : ICommandHandler<RefreshTokenCommand, LoginResponseDto>
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    public RefreshTokenHandler(UserManager<User> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<LoginResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user is null) throw new BadRequestException("Your token is not valid.");

        var refreshToken = await _userManager.GetAuthenticationTokenAsync(user, "Identity", "RefreshToken");
        bool isValid = await _userManager.VerifyUserTokenAsync(user, "Identity", "RefreshToken", request.token);
        if (!refreshToken.Equals(request.token) || !isValid) throw new BadRequestException($"Your token is not valid.");

        var userRoles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
        var accesstoken = new JwtSecurityTokenHandler().WriteToken(await _tokenService.GenerateJWToken(user, userRoles.ToList()));
        var refershToken = await _tokenService.GenerateRefreshToken(user);

        return new LoginResponseDto(user.Id, $"{user.FirstName} {user.LastName}", user.UserName, userRoles.ToList(), accesstoken, refershToken);
    }
}
public class RefreshToken : IMinimalEndpoint
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost($"{EndpointConfig.BaseApiPath}/identity/refresh-token", async (
                RefreshTokenCommand request, IMediator mediator, IMapper mapper,
                CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(request, cancellationToken);
            return Results.Ok(result);
        })
            .WithName("refresh token")
            .WithApiVersionSet(builder.NewApiVersionSet("auth").Build())
            .Produces<LoginResponseDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("refresh token")
            .WithDescription("refresh token")
            .WithOpenApi()
            .HasApiVersion(1.0);

        return builder;
    }
}