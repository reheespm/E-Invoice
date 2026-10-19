using EInvoice.Domain.Entities;
using EInvoice.Infrastructure.Identity;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EInvoice.Infrastructure.Persistence;

public class EInvoiceDbContext : IdentityDbContext<
    ApplicationUser,
    ApplicationRole,
    Guid,
    IdentityUserClaim<Guid>,
    IdentityUserRole<Guid>,
    IdentityUserLogin<Guid>,
    IdentityRoleClaim<Guid>,
    IdentityUserToken<Guid>>, IDataProtectionKeyContext
{
    public EInvoiceDbContext(DbContextOptions<EInvoiceDbContext> options) : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<UserCompany> UserCompanies => Set<UserCompany>();

    public DbSet<Country> Countries => Set<Country>();

    public DbSet<City> Cities => Set<City>();

    public DbSet<UserDashboardWidget> UserDashboardWidgets => Set<UserDashboardWidget>();

    public DbSet<UserQuickLink> UserQuickLinks => Set<UserQuickLink>();

    /// <summary>
    /// ASP.NET Core Data Protection keys (used to sign the login cookie and anti-forgery tokens).
    /// Stored in the database so they survive restarts/redeploys and are shared by every server instance.
    /// </summary>
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Company>(entity =>
        {
            entity.ToTable("Companies");
            entity.Property(c => c.Name).HasMaxLength(200).IsRequired();
            entity.Property(c => c.Address).HasMaxLength(300).IsRequired();
            entity.Property(c => c.Street).HasMaxLength(200);
            entity.Property(c => c.PoBox).HasMaxLength(50);
            entity.Property(c => c.Phone).HasMaxLength(50).IsRequired();
            entity.Property(c => c.Email).HasMaxLength(256);
            entity.Property(c => c.Website).HasMaxLength(256);
            entity.Property(c => c.CreatedBy).HasMaxLength(256);
            entity.Property(c => c.ModifiedBy).HasMaxLength(256);
            entity.HasOne<City>().WithMany().HasForeignKey(c => c.CityId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Country>().WithMany().HasForeignKey(c => c.CountryId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.Property(c => c.Name).HasMaxLength(200).IsRequired();
            entity.Property(c => c.Address1).HasMaxLength(300).IsRequired();
            entity.Property(c => c.Street).HasMaxLength(200);
            entity.Property(c => c.PostBox).HasMaxLength(50);
            entity.Property(c => c.Trn).HasMaxLength(50);
            entity.Property(c => c.Tin).HasMaxLength(50);
            entity.Property(c => c.PeppolId).HasMaxLength(200);
            entity.Property(c => c.Phone).HasMaxLength(50).IsRequired();
            entity.Property(c => c.Email).HasMaxLength(256);
            entity.Property(c => c.Website).HasMaxLength(256);
            entity.Property(c => c.CreatedBy).HasMaxLength(256);
            entity.Property(c => c.ModifiedBy).HasMaxLength(256);
            entity.HasOne<Company>().WithMany().HasForeignKey(c => c.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<City>().WithMany().HasForeignKey(c => c.CityId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Country>().WithMany().HasForeignKey(c => c.CountryId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Country>(entity =>
        {
            entity.ToTable("Countries");
            entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
            entity.Property(c => c.CreatedBy).HasMaxLength(256);
            entity.Property(c => c.ModifiedBy).HasMaxLength(256);
            entity.HasIndex(c => c.Name).IsUnique();
        });

        builder.Entity<City>(entity =>
        {
            entity.ToTable("Cities");
            entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
            entity.Property(c => c.CreatedBy).HasMaxLength(256);
            entity.Property(c => c.ModifiedBy).HasMaxLength(256);
            entity.HasOne<Country>().WithMany().HasForeignKey(c => c.CountryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(c => new { c.CountryId, c.Name }).IsUnique();
        });

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FirstName).HasMaxLength(100);
            entity.Property(u => u.LastName).HasMaxLength(100);
            entity.ToTable("Users");
        });

        builder.Entity<ApplicationRole>(entity =>
        {
            entity.Property(r => r.Description).HasMaxLength(250);
            entity.ToTable("Roles");
        });

        builder.Entity<UserCompany>(entity =>
        {
            entity.ToTable("UserCompanies");
            entity.HasKey(uc => new { uc.UserId, uc.CompanyId });
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(uc => uc.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Company>().WithMany().HasForeignKey(uc => uc.CompanyId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<UserDashboardWidget>(entity =>
        {
            entity.ToTable("UserDashboardWidgets");
            entity.HasKey(w => new { w.UserId, w.WidgetKey });
            entity.Property(w => w.WidgetKey).HasMaxLength(100);
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(w => w.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<UserQuickLink>(entity =>
        {
            entity.ToTable("UserQuickLinks");
            entity.HasKey(l => new { l.UserId, l.LinkKey });
            entity.Property(l => l.LinkKey).HasMaxLength(100);
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(l => l.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<IdentityUserRole<Guid>>(e => e.ToTable("UserRoles"));
        builder.Entity<IdentityUserClaim<Guid>>(e => e.ToTable("UserClaims"));
        builder.Entity<IdentityUserLogin<Guid>>(e => e.ToTable("UserLogins"));
        builder.Entity<IdentityRoleClaim<Guid>>(e => e.ToTable("RoleClaims"));
        builder.Entity<IdentityUserToken<Guid>>(e => e.ToTable("UserTokens"));
    }
}
