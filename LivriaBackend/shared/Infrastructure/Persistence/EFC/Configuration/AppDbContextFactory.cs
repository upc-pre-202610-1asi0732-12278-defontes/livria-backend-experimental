using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LivriaBackend.shared.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
/// Permite que <c>dotnet ef</c> use la misma cadena que Program.cs (.env + DbName).
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        LivriaEnvLoader.Load();

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
        var finalConnectionString = LivriaConnectionString.Build(baseCredentials, dbName);

        Console.WriteLine($"[EF Design-Time] DbName config: {dbName}");
        Console.WriteLine($"[EF Design-Time] Connection: {LivriaConnectionString.RedactForLog(finalConnectionString)}");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseMySQL(finalConnectionString);
        return new AppDbContext(optionsBuilder.Options);
    }

    private static IEnumerable<string> EnumerateDirectoriesUp(string start)
    {
        var current = start;
        while (!string.IsNullOrEmpty(current))
        {
            yield return current;
            var parent = Directory.GetParent(current);
            current = parent?.FullName ?? string.Empty;
        }
    }

    private static string ResolveConfigurationBasePath()
    {
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            foreach (var dir in EnumerateDirectoriesUp(start))
            {
                if (File.Exists(Path.Combine(dir, "appsettings.json")))
                    return dir;
                if (File.Exists(Path.Combine(dir, "LivriaBackend", "appsettings.json")))
                    return Path.Combine(dir, "LivriaBackend");
            }
        }

        throw new InvalidOperationException(
            "No se encontró appsettings.json. Ejecuta desde la raíz del repo o LivriaBackend.");
    }
}
