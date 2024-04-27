namespace Identity.Identity.Model;

public class UserTenant
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public virtual User User { get; set; }
    public virtual Tenant Tenant { get; set; }
}
