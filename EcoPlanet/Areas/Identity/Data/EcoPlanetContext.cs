using EcoPlanet.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EcoPlanet.Models;

namespace EcoPlanet.Data;

public class EcoPlanetContext : IdentityDbContext<EcoPlanetUser>
{
    public EcoPlanetContext(DbContextOptions<EcoPlanetContext> options)
        : base(options)
    {
    }

    public DbSet<Trashpedia> TrashpediaTable { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }
}
