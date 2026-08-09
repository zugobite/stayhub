using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StayHub.Data;

/// <summary>
/// Design-time factory used by <c>dotnet ef</c>. It lets migrations be added and
/// scripted from the Data project without booting the web host.
/// </summary>
public class StayHubDbContextFactory : IDesignTimeDbContextFactory<StayHubDbContext>
{
    /// <summary>
    /// Default connection string, matching the SQL Server container in
    /// <c>docker-compose.yml</c>. Override it with the <c>STAYHUB_CONNECTION</c>
    /// environment variable when pointing at another instance.
    /// </summary>
    private const string DefaultConnection =
        "Server=localhost,1433;Database=StayHubDB;User Id=sa;Password=Password123!;TrustServerCertificate=True;";

    /// <inheritdoc />
    public StayHubDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("STAYHUB_CONNECTION") ?? DefaultConnection;

        var options = new DbContextOptionsBuilder<StayHubDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new StayHubDbContext(options);
    }
}
