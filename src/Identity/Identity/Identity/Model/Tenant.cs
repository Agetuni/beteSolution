namespace Identity.Identity.Model;

public class Tenant 
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
}
