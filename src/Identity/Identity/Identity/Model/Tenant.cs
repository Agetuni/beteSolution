namespace Identity.Identity.Model;

public class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Code { get; set; }

    public static Tenant Create(string name, string code)
    {
        return new Tenant { Code = code, Name = name };
    }

}
