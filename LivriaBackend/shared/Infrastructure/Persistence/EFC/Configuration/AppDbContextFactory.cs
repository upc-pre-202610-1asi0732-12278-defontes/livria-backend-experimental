using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LivriaBackend.shared.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
/// Permite que <c>dotnet ef</c> use la misma cadena que Program.cs (.env + DbName).
/// Sin esto, las migraciones caen en appsettings.json → livriadb.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        Env.TraversePath().Load();

        var basePath = ResolveConfigurationBasePath();
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddUserSecrets(typeof(AppDbContextFactory).Assembly, optional: true)
            .AddEnvironmentVariables()
            .Build();

        var baseCredentials = configuration.GetConnectionString("DefaultConnection");
        var dbName = configuration.GetConnectionString("DbName");
        var finalConnectionString = $"{baseCredentials}database={dbName};";

        if (string.IsNullOrWhiteSpace(baseCredentials))
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection vacío. Copia .env.example → .env en la raíz del repo " +
                "o define ConnectionStrings__DefaultConnection antes de dotnet ef.");

        if (string.IsNullOrWhiteSpace(dbName))
            throw new InvalidOperationException(
                "ConnectionStrings:DbName vacío. En experimental debe ser livriadb_experimental (.env).");

        Console.WriteLine($"[EF Design-Time] Target database: {dbName}");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseMySQL(finalConnectionString);
        return new AppDbContext(optionsBuilder.Options);
    }

    private static string ResolveConfigurationBasePath()
    {
        var candidates = new[]
        {
            Directory.GetCurrentDirectory(),
            Path.Combine(Directory.GetCurrentDirectory(), "LivriaBackend"),
            AppContext.BaseDirectory
        };

        foreach (var dir in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(dir))
                continue;
            if (File.Exists(Path.Combine(dir, "appsettings.json")))
                return dir;
        }

        throw new InvalidOperationException(
            "No se encontró appsettings.json. Ejecuta desde la raíz del repo o desde LivriaBackend.");
    }
}
