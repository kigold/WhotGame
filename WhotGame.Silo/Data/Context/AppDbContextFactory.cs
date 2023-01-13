using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace WhotGame.Silo.Data.Context
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<AppDbContext>();
            builder.EnableSensitiveDataLogging(true);
            var connectionString = "Data Source=.;Initial Catalog=approvalEngine;Integrated Security=true;";
            builder.UseSqlServer(connectionString, b => b.MigrationsAssembly(this.GetType().Assembly.FullName));
            var dbContext = (AppDbContext)Activator.CreateInstance(typeof(AppDbContext), builder.Options);
            return dbContext;
        }
    }
}
