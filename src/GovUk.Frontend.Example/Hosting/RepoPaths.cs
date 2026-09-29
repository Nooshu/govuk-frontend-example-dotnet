namespace GovUk.Frontend.Example.Hosting;

public static class RepoPaths
{
    public static string PolicyFile() =>
        Require(
            Find(["baseline", "policy.json"], SearchRoots(AppContext.BaseDirectory, Directory.GetCurrentDirectory())),
            "baseline/policy.json");

    public static string ComponentFixturesDirectory() =>
        FixturesFromRoot(
            Environment.GetEnvironmentVariable("GOVUK_FRONTEND_ROOT"),
            FindDirectoryContaining(
                "package.json",
                SearchRoots(AppContext.BaseDirectory, Directory.GetCurrentDirectory())));

    internal static string FixturesFromRoot(string? configuredRoot, string? repoRoot)
    {
        if (!string.IsNullOrWhiteSpace(configuredRoot))
        {
            return Path.Combine(configuredRoot, "dist", "govuk", "components");
        }

        return Path.Combine(
            Require(repoRoot, "the repository root"),
            "node_modules",
            "govuk-frontend",
            "dist",
            "govuk",
            "components");
    }

    internal static string Require(string? found, string description) =>
        found ?? throw new InvalidOperationException($"Could not find {description}.");

    internal static string? Find(IReadOnlyList<string> relative, IEnumerable<string> starts)
    {
        foreach (var start in starts)
        {
            var directory = new DirectoryInfo(start);
            while (directory is not null)
            {
                var candidate = Path.Combine(new[] { directory.FullName }.Concat(relative).ToArray());
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }
        }

        return null;
    }

    internal static string? FindDirectoryContaining(string fileName, IEnumerable<string> starts)
    {
        foreach (var start in starts)
        {
            var directory = new DirectoryInfo(start);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, fileName)))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }
        }

        return null;
    }

    internal static IEnumerable<string> SearchRoots(string baseDirectory, string currentDirectory)
    {
        yield return baseDirectory;
        if (!IsSameOrChild(baseDirectory, currentDirectory))
        {
            yield return currentDirectory;
        }
    }

    private static bool IsSameOrChild(string path, string ancestor)
    {
        var fullPath = Path.GetFullPath(path);
        var fullAncestor = Path.GetFullPath(ancestor).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return fullPath.Equals(Path.GetFullPath(ancestor), StringComparison.Ordinal)
            || fullPath.StartsWith(fullAncestor, StringComparison.Ordinal);
    }
}
