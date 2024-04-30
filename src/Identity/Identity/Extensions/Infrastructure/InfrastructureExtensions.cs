using BuildingBlocks.EFCore;
using BuildingBlocks.Web;
using Figgle;
using Identity.Data.Seed;
using Identity.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using BuildingBlocks.Logging;
using BuildingBlocks.Swagger;
using FluentValidation;
using BuildingBlocks.Mapster;
using BuildingBlocks.HealthCheck;
using Microsoft.AspNetCore.HttpOverrides;
using BuildingBlocks.Exceptions;
using Serilog;
using Microsoft.Extensions.Hosting;
using BuildingBlocks.Jwt;

namespace Identity.Extensions.Infrastructure;

public static class InfrastructureExtensions
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var env = builder.Environment;

        builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();

        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        var appOptions = builder.Services.GetOptions<AppOptions>(nameof(AppOptions));
        Console.WriteLine(FiggleFonts.Standard.Render(appOptions.Name));

        builder.Services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 10,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    }));
        });
        builder.Services.AddEndpointsApiExplorer();
        builder.AddCustomSerilog(env);
        builder.Services.AddCustomDbContext<IdentityContext>();
        builder.Services.AddScoped<IDataSeeder, IdentityDataSeeder>();
        builder.Services.AddCustomSwagger(configuration, typeof(IdentityRoot).Assembly);
        builder.Services.AddCustomVersioning();
        builder.Services.AddCustomMediatR();
        builder.Services.AddValidatorsFromAssembly(typeof(IdentityRoot).Assembly);
        builder.Services.AddProblemDetails();
        builder.Services.AddCustomMapster(typeof(IdentityRoot).Assembly);
        builder.Services.AddCustomHealthCheck();
        builder.AddCustomIdentityServer(configuration);
        builder.AddJwtCustomExtensions(configuration);
        builder.Services.AddCors();
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        return builder;
    }


    public static WebApplication UseInfrastructure(this WebApplication app)
    {
        var env = app.Environment;
        var appOptions = app.GetOptions<AppOptions>(nameof(AppOptions));

        //cors


        app.UseForwardedHeaders();

        app.UseCustomProblemDetails();
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = LogEnrichHelper.EnrichFromRequest;
        });
        app.UseMigration<IdentityContext>(env);
        app.UseCustomHealthCheck();

        app.MapGet("/", x => x.Response.WriteAsync(appOptions.Name));

        if (env.IsDevelopment())
        {
            app.UseCustomSwagger();
        }
        app.UseCors(builder =>
        {
            builder.WithOrigins("https://localhost:3000","http://localhost:3000")
                .AllowAnyMethod()                    // Allow any HTTP method
                   .AllowAnyHeader()                    // Allow any HTTP headers
                   .AllowCredentials();                 // Allow credentials (e.g., cookies, authorization headers)
        });
        return app;
    }
}
