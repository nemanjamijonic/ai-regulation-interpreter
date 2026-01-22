
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Common.Context
{
    public class AIRegulationInterpreterContextFactory : IDesignTimeDbContextFactory<AIRegulationInterpreterContext>
    {
        public AIRegulationInterpreterContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json")
               .Build();

            var builder = new DbContextOptionsBuilder<AIRegulationInterpreterContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            builder.UseSqlServer(connectionString);

            return new AIRegulationInterpreterContext(builder.Options);
        }
    }
}
