using Identity.Identity.Model;

namespace Identity.Data.Seed;

public static class InitialData
{
    public static List<User> Users { get; set; }
    public static Tenant Tenant { get; set; }

    static InitialData()
    {
        Users = new List<User>()
        {
            new User
            {Id= new Guid(),
                FirstName= "Aleazar",
                LastName= "Yilma",
                UserName="Aleazar",
                Email= "abcd@gmail.com",
                SecurityStamp = Guid.NewGuid().ToString()
            },
            new User
            {
                Id= new Guid(),
                FirstName= "Raymond",
                LastName= "Redington ",
                UserName="Raymond",
                Email= "efgh@gmail.com",
                SecurityStamp = Guid.NewGuid().ToString()

            }
        };

        Tenant = new Tenant
        {
            Id = new Guid(),
            Name = "AlazarTenant",
            Code = "AT"
        };

    }
}
