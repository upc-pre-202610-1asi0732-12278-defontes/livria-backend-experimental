using System.Text.RegularExpressions;

namespace LivriaBackend.shared.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
/// Arma la cadena final: credenciales + database={DbName}.
/// Quita cualquier Database/database previo en DefaultConnection (p. ej. user-secrets con livriadb).
/// </summary>
public static class LivriaConnectionString
{
    private static readonly Regex DatabaseKeyPattern = new(
        @"(?i)^\s*(database|initial\s*catalog)\s*=",
        RegexOptions.Compiled);

    public static string Build(string? baseCredentials, string? dbName)
    {
        if (string.IsNullOrWhiteSpace(baseCredentials))
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing or empty.");

        if (string.IsNullOrWhiteSpace(dbName))
            throw new InvalidOperationException("ConnectionStrings:DbName is missing or empty.");

        var cleaned = StripDatabaseKey(baseCredentials.Trim());
        if (!cleaned.EndsWith(';'))
            cleaned += ";";

        var targetDb = dbName.Trim();
        if (targetDb.Equals("livriadb", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "DbName es 'livriadb'. En livria-backend-experimental usa 'livriadb_experimental'. " +
                "Revisa .env (ConnectionStrings__DbName) y quita Database= de DefaultConnection / user-secrets.");
        }

        var connection = $"{cleaned}database={targetDb};";
        return EnsureAllowUserVariables(connection);
    }

    /// <summary>
    /// Migraciones idempotentes usan variables MySQL (@var, PREPARE). Requiere Allow User Variables=True.
    /// </summary>
    public static string EnsureAllowUserVariables(string connectionString)
    {
        if (Regex.IsMatch(connectionString, @"(?i)allow\s*user\s*variables\s*="))
            return connectionString;

        if (!connectionString.EndsWith(';'))
            connectionString += ";";

        return $"{connectionString}Allow User Variables=True;";
    }

    public static string StripDatabaseKey(string connectionString)
    {
        var parts = connectionString
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(part => !DatabaseKeyPattern.IsMatch(part));

        return string.Join(';', parts);
    }

    public static string RedactForLog(string connectionString) =>
        Regex.Replace(connectionString, @"(?i)(Password\s*=\s*)[^;]*", "$1***");
}
