using Ardalis.GuardClauses;
using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web;
using FluentValidation;
using Identity.Data;
using Identity.Identity.Model;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Identity.Identity.Features;


//response 
public record CreateTenantResponse(Guid Id);

//command
public record CreateTenantCommand(string Name, string Code) : ICommand<CreateTenantResponse>;

//validation
public class CreateTenantValidation : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantValidation(IdentityContext _context)
    {
        RuleFor(x => x.Code)
            .NotEmpty()
                .WithMessage("please enter code.")
            .MustAsync(async (name, ct) => await _context.Tenant.FirstOrDefaultAsync(x => x.Name == name) is null)
                .WithMessage((name) => $"Tenant {name} already exists.");

        RuleFor(x => x.Name).NotEmpty().WithMessage("please enter name.");
    }
}

//handler 
public class CreateTenantHandler : ICommandHandler<CreateTenantCommand, CreateTenantResponse>
{
    private readonly IdentityContext _context;
    public CreateTenantHandler(IdentityContext context) => _context = context;

    public async Task<CreateTenantResponse> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        Guard.Against.Null(request, nameof(request));
        var tenant = Tenant.Create(request.Name, request.Code);
        var newTenant = (await _context.Tenant.AddAsync(tenant)).Entity;
        _context.SaveChanges();
        return new CreateTenantResponse(newTenant.Id);
    }
}


public class CreateTenant : IMinimalEndpoint
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost($"{EndpointConfig.BaseApiPath}/tenant", async (CreateTenantCommand request, IMediator mediator, CancellationToken cancelationToken) =>
        {
            var result = await mediator.Send(request, cancelationToken);
            return Results.CreatedAtRoute("GetTenantById", new { id = result.Id }, result);
        })
        .WithName("CreateTenant")
        .RequireAuthorization()
        .WithApiVersionSet(builder.NewApiVersionSet("Tenant").Build())
        .Produces<CreateTenantResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Create Tenant")
        .WithDescription("Create Tenant")
        .WithOpenApi()
        .HasApiVersion(1.0);
        return builder;
    }
}