using System.Reflection;

namespace TestData;

/// <summary>
/// Common helper utilities for tests across the solution.
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Finds the solution directory by walking up from the assembly location until a .sln file is found.
    /// This is more robust than using relative paths with "..".
    /// </summary>
    /// <returns>The solution directory path, or null if no .sln file is found.</returns>
    public static string? TryGetSolutionDirectory()
    {
        var assemblyPath = Assembly.GetExecutingAssembly().Location;
        var directory = Path.GetDirectoryName(assemblyPath);

        return WalkUpToSolutionDirectory(directory);
    }

    private static string? WalkUpToSolutionDirectory(string? directory)
    {
        while (directory != null)
        {
            if (HasSolutionFile(directory))
            {
                return directory;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        return null;
    }

    private static bool HasSolutionFile(string directory)
    {
        return Directory.GetFiles(directory, "*.sln").Any();
    }
}
