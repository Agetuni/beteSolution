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
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Identity.Identity.Features.Auth;

//command
public record MeCommand() : ICommand<MiniUserDto>;

//response

public record MiniUserDto(Guid Id, string Name, string UserName, List<string> Roles);

// handler 
public class MeCommandHandler : ICommandHandler<MeCommand, MiniUserDto>
{
    private readonly ITokenService _tokenService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly ICurrentUserProvider _currentUserProvider;
    public MeCommandHandler(ITokenService tokenService, ICurrentUserProvider currentUserProvider, IHttpContextAccessor httpContextAccessor)
    {
        _tokenService = tokenService;
        _currentUserProvider = currentUserProvider;
        _httpContextAccessor = httpContextAccessor;

    }
    public async Task<MiniUserDto> Handle(MeCommand request, CancellationToken cancellationToken)
    {
        var user = _httpContextAccessor?.HttpContext?.User;
        if (user == null) throw new UnauthorizedAccessException("user is empty");
        var username = user.FindFirstValue("username");
        var fullNmae = user.FindFirstValue("fullNmae");
        var uid = user.FindFirstValue("uid");
        var roles = user.Claims
            .Where(x => x.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
            .Select(x => x.Value)
            .ToList();
        return new MiniUserDto(new Guid(uid), fullNmae, username, roles);
    }
}
public class Me : IMinimalEndpoint
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet($"{EndpointConfig.BaseApiPath}/identity/me", async (IMediator mediator, IMapper mapper,
                CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new MeCommand(), cancellationToken);
            return Results.Ok(result);
        })
            .WithName("me")
            .WithApiVersionSet(builder.NewApiVersionSet("auth").Build())
            .Produces<LoginResponseDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("get user data")
            .WithDescription("get user data")
            .WithOpenApi()
            .HasApiVersion(1.0)
            .RequireAuthorization("login");

        return builder;
    }
}