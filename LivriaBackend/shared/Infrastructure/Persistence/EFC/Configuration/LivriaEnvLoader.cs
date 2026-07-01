using DotNetEnv;

namespace LivriaBackend.shared.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
/// Carga .env desde la raíz del repo experimental (carpeta que contiene LivriaBackend/ y .env).
/// </summary>
public static class LivriaEnvLoader
{
    public static string? LoadedFrom { get; private set; }

    public static void Load()
    {
        foreach (var dir in EnumerateDirectoriesUp(Directory.GetCurrentDirectory()))
        {
            var envPath = Path.Combine(dir, ".env");
            if (!File.Exists(envPath))
                continue;

            Env.Load(envPath);
            LoadedFrom = envPath;
            Console.WriteLine($"[Livria] Loaded .env from {envPath}");
            return;
        }

        Env.TraversePath().Load();
        LoadedFrom = "(TraversePath)";
        Console.WriteLine("[Livria] Loaded .env via TraversePath");
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
}
