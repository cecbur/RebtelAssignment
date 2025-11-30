using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace ArchitectureTests.MicroserviceEnforcingTests;

public abstract class MicroserviceArchitectureTestBase
{
    protected const string ProjectFilePattern = "*.csproj";
    protected const string SourceFilePattern = "*.cs";
    protected const string BinDirectory = "bin";
    protected const string ObjDirectory = "obj";
    protected const string ProjectReferenceElement = "ProjectReference";
    protected const string IncludeAttribute = "Include";
    protected const string ArchitectureTestsProjectName = "ArchitectureTests.csproj";

    private string? _solutionDirectory;

    [SetUp]
    public void SetUp()
    {
        _solutionDirectory = TryGetSolutionDirectory();

        if (_solutionDirectory == null)
        {
            Assert.Ignore("Solution directory not found. This test requires access to the full solution structure with a .sln file.");
        }
    }

    protected List<(string ProjectPath, string ReferenceLine)> CollectAllViolations(string[] allowedReferencingProjects)
    {
        var violations = new List<(string ProjectPath, string ReferenceLine)>();

        CheckProjectReferences(allowedReferencingProjects, violations);
        CheckAllowedProjectsUsage(allowedReferencingProjects, violations);

        return violations;
    }

    private void CheckProjectReferences(
        string[] allowedReferencingProjects,
        List<(string ProjectPath, string ReferenceLine)> violations)
    {
        var projectFiles = GetAllProjectFiles();

        foreach (var projectFile in projectFiles)
        {
            CheckProjectFileForViolations(projectFile, allowedReferencingProjects, violations);
        }
    }

    private string[] GetAllProjectFiles()
    {
        var projectsToExclude = GetProjectsToExcludeFromScanning();

        var projectFiles = Directory.GetFiles(_solutionDirectory!, ProjectFilePattern, SearchOption.AllDirectories)
            .Where(f =>
            {
                var filename = Path.GetFileName(f);
                return !projectsToExclude.Contains(filename);
            })
            .ToArray();
        return projectFiles;
    }

    private void CheckProjectFileForViolations(
        string projectFile,
        string[] allowedReferencingProjects,
        List<(string ProjectPath, string ReferenceLine)> violations)
    {
        var projectName = Path.GetFileName(projectFile);

        if (ShouldSkipProject(projectName, allowedReferencingProjects))
        {
            return;
        }

        var microserviceReferences = GetMicroserviceReferences(projectFile);
        AddProjectReferenceViolations(projectFile, microserviceReferences, violations);
    }

    private bool ShouldSkipProject(string projectName, string[] allowedReferencingProjects)
    {
        var isMicroserviceProject = projectName == GetMicroserviceProjectName();
        var isAllowedProject = allowedReferencingProjects.Contains(projectName);

        return isMicroserviceProject || isAllowedProject;
    }

    private List<XElement> GetMicroserviceReferences(string projectFile)
    {
        try
        {
            var projectContent = File.ReadAllText(projectFile);
            var doc = XDocument.Parse(projectContent);

            var microserviceReferences = doc.Descendants(ProjectReferenceElement)
                .Where(pr => pr.Attribute(IncludeAttribute)?.Value.Contains(GetMicroserviceProjectName()) == true)
                .ToList();

            return microserviceReferences;
        }
        catch (XmlException ex)
        {
            Assert.Fail($"Failed to parse project file '{projectFile}': {ex.Message}");
            return new List<XElement>(); // Never reached but satisfies compiler
        }
    }

    private static void AddProjectReferenceViolations(
        string projectFile,
        List<XElement> microserviceReferences,
        List<(string ProjectPath, string ReferenceLine)> violations)
    {
        foreach (var reference in microserviceReferences)
        {
            var referenceLine = reference.ToString();
            violations.Add((projectFile, referenceLine));
        }
    }

    private void CheckAllowedProjectsUsage(
        string[] allowedReferencingProjects,
        List<(string ProjectPath, string ReferenceLine)> violations)
    {
        foreach (var projectName in allowedReferencingProjects)
        {
            CheckProjectSourceFilesForViolations(projectName, violations);
        }
    }

    private void CheckProjectSourceFilesForViolations(
        string projectName,
        List<(string ProjectPath, string ReferenceLine)> violations)
    {
        var projectDirectory = GetProjectDirectory(projectName);

        if (projectDirectory == null)
        {
            return;
        }

        var sourceFiles = GetSourceFilesExcludingBuildFolders(projectDirectory);

        foreach (var sourceFile in sourceFiles)
        {
            CheckFileForMicroserviceImports(sourceFile, violations);
        }
    }

    private string? GetProjectDirectory(string projectName)
    {
        var projectDirName = Path.GetFileNameWithoutExtension(projectName);
        var projectDir = Path.Combine(_solutionDirectory!, projectDirName);

        if (!Directory.Exists(projectDir))
        {
            return null;
        }

        return projectDir;
    }

    private static List<string> GetSourceFilesExcludingBuildFolders(string projectDirectory)
    {
        var sourceFiles = Directory.GetFiles(projectDirectory, SourceFilePattern, SearchOption.AllDirectories)
            .Where(f =>
            {
                var pathSegments = f.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return !pathSegments.Contains(BinDirectory) && !pathSegments.Contains(ObjDirectory);
            })
            .ToList();

        return sourceFiles;
    }

    private void CheckFileForMicroserviceImports(
        string sourceFile,
        List<(string ProjectPath, string ReferenceLine)> violations)
    {
        var lines = File.ReadLines(sourceFile);
        var lineNumber = 0;

        foreach (var line in lines)
        {
            lineNumber++;
            var trimmedLine = line.Trim();

            if (IsMicroserviceNamespaceImport(trimmedLine))
            {
                var violationMessage = $"Line {lineNumber}: {trimmedLine} - Direct {GetMicroserviceName()} namespace import not allowed";
                violations.Add((sourceFile, violationMessage));
            }
        }
    }

    private bool IsMicroserviceNamespaceImport(string line)
    {
        var isExactMatch = line.StartsWith(GetMicroserviceNamespaceExact());
        var isSubNamespace = line.StartsWith(GetMicroserviceNamespacePrefix());

        var isViolation = isExactMatch || isSubNamespace;
        return isViolation;
    }

    protected static string BuildErrorMessage(List<(string ProjectPath, string ReferenceLine)> violations, string architecturalViolationMessage)
    {
        var violationsSummary = FormatViolations(violations);
        var errorMessage = architecturalViolationMessage.Replace("{violations}", violationsSummary);

        return errorMessage;
    }

    private static string FormatViolations(List<(string ProjectPath, string ReferenceLine)> violations)
    {
        var formattedViolations = violations.Select(v => $"  File: {v.ProjectPath}\n  Issue: {v.ReferenceLine}");
        var violationsSummary = string.Join("\n\n", formattedViolations);

        return violationsSummary;
    }

    private static string? TryGetSolutionDirectory()
    {
        var assemblyPath = Assembly.GetExecutingAssembly().Location;
        var directory = Path.GetDirectoryName(assemblyPath);

        var solutionDirectory = WalkUpToSolutionDirectory(directory);
        return solutionDirectory;
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
        var solutionFiles = Directory.GetFiles(directory, "*.sln");
        var hasSolutionFile = solutionFiles.Any();

        return hasSolutionFile;
    }

    // Abstract methods that derived classes must implement
    protected abstract string GetMicroserviceProjectName();
    protected abstract string GetMicroserviceGrpcClientName();
    protected abstract string GetMicroserviceNamespaceExact();
    protected abstract string GetMicroserviceNamespacePrefix();
    protected abstract string GetMicroserviceName();
    protected abstract string[] GetAllowedReferencingProjectsForDi();
    protected abstract string GetArchitecturalViolationMessage(string[] allowedProjects);
    protected abstract string[] GetProjectsToExcludeFromScanning();
}
