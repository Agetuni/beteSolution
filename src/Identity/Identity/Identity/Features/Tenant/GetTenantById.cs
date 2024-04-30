using Ardalis.GuardClauses;
using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web;
using FluentValidation;
using Identity.Data;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using NotFoundException = BuildingBlocks.NotFoundException;

namespace Identity.Identity.Features;
public record TenantResponse(Guid Id, string Name, string Code);
public record GetTenantByIdCommand(Guid Id) : ICommand<TenantResponse>;
public class GetTenantByIdValidator : AbstractValidator<GetTenantByIdCommand>
{
    public GetTenantByIdValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required");

    }
}

public class GetTenantByIdHandler : ICommandHandler<GetTenantByIdCommand, TenantResponse>
{
    private readonly IdentityContext _context;
    public GetTenantByIdHandler(IdentityContext context)
    {
        _context = context;
    }
    public async Task<TenantResponse> Handle(GetTenantByIdCommand request, CancellationToken cancellationToken)
    {
        Guard.Against.Null(request, nameof(request));
        var tenant = await _context.Tenant.FirstOrDefaultAsync(x => x.Id == request.Id);
        if (tenant == null) throw new NotFoundException("Tenant not found ");

        return new TenantResponse(tenant.Id, tenant.Name, tenant.Code);
    }
}

public class GetTenantById : IMinimalEndpoint
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet($"{EndpointConfig.BaseApiPath}/tenant/{{id}}",
            async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                var result = await mediator.Send(new GetTenantByIdCommand(id), ct);
                return Results.Ok(result);
            })
            .WithName("GetTenantById")
            .WithApiVersionSet(builder.NewApiVersionSet("Tenant").Build())
            .Produces<TenantResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Tenant By Id")
            .WithDescription("Get Tenant By Id")
            .WithOpenApi()
            .HasApiVersion(1.0);

        return builder;
    }

}
