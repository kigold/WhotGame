using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace WhotGame.Silo.Data.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
        {

        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRole { get; set; }
        public DbSet<UserClaim> UserClaims { get; set; }
        public DbSet<IdentityRoleClaim<long>> RoleClaims { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);


            builder.UseOpenIddict<long>();
            builder.Entity<UserRole>().HasKey(x => new { x.UserId, x.RoleId });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var config = new ConfigurationBuilder()
                          .AddEnvironmentVariables()
                         .Build();

            IConfigurationRoot configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddJsonFile("appsettings.Development.json", optional: true)
               .AddJsonFile($"appsettings.{config["ASPNETCORE_ENVIRONMENT"]}.json", optional: true)
               .Build();
            //var connectionString = "Data Source=.;Initial Catalog=approvalEngine;Integrated Security=true;";
            var connectionString = configuration["ConnectionStrings:Default"];
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
