using BuildingBlocks.Web;
using Identity;
using Identity.Extensions.Infrastructure;

//builder
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseDefaultServiceProvider((context, options) =>
{
    #region reference
    // Service provider validation
    // ref: https://andrewlock.net/new-in-asp-net-core-3-service-provider-validation/
    #endregion
    options.ValidateScopes = context.HostingEnvironment.IsDevelopment() || context.HostingEnvironment.IsStaging() || context.HostingEnvironment.IsEnvironment("tests");
    options.ValidateOnBuild = true;
});
builder.AddMinimalEndpoints(assemblies: typeof(IdentityRoot).Assembly);
builder.AddInfrastructure();

//app
var app = builder.Build();
app.UseInfrastructure();
app.MapMinimalEndpoints();
app.Run();

// Hola!
namespace Identity.Api
{
    public partial class Program { }
}
