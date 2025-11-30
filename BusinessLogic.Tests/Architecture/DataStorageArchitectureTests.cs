using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace BusinessLogic.Tests.Architecture;

public class DataStorageArchitectureTests
{
    private const string ProjectFilePattern = "*.csproj";
    private const string SourceFilePattern = "*.cs";
    private const string BinDirectory = "bin";
    private const string ObjDirectory = "obj";
    private const string DataStorageNamespaceExact = "using DataStorage;";
    private const string DataStorageNamespacePrefix = "using DataStorage.";
    private const string ProjectReferenceElement = "ProjectReference";
    private const string IncludeAttribute = "Include";
    private const string DataStorageProjectName = "DataStorage.csproj";
    private const string DataStorageGrpcClientName = "DataStorageGrpcClient.csproj";

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

    [Test]
    public void DataStorage_ShouldOnlyBeAccessedThroughGrpcClient()
    {
        var allowedReferencingProjects = GetAllowedReferencingProjectsForDi();
        var violations = CollectAllViolations(allowedReferencingProjects);

        var errorMessage = BuildErrorMessage(violations);
        Assert.That(violations, Is.Empty, errorMessage);

    }

    /// <summary>
    /// Returns the list of projects allowed to have a ProjectReference to DataStorage.csproj.
    /// These projects are typically composition roots that set up DI for both server and client.
    /// They must use DataStorageGrpcClient.Setup extension methods and should not import the DataStorage namespace in source files (except for IDbConnectionFactory usage).
    /// </summary>
    /// <returns>Array of .csproj file names that are allowed to reference DataStorage</returns>
    private static string[] GetAllowedReferencingProjectsForDi()
    {
        return
        [
            "LibraryApi.csproj", // Composition root - sets up DI for both server and client
            "BusinessLogic.csproj", // BusinessLogic references DataStorage for repository access
        ];
    }

    private List<(string ProjectPath, string ReferenceLine)> CollectAllViolations(string[] allowedReferencingProjects)
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
        var projectFiles = Directory.GetFiles(_solutionDirectory!, ProjectFilePattern, SearchOption.AllDirectories)
            .Where(f =>
            {
                var filename = Path.GetFileName(f);
                return filename != DataStorageProjectName &&
                       filename != DataStorageGrpcClientName;
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

        var dataStorageReferences = GetDataStorageReferences(projectFile);
        AddProjectReferenceViolations(projectFile, dataStorageReferences, violations);
    }

    private static bool ShouldSkipProject(string projectName, string[] allowedReferencingProjects)
    {
        var isDataStorageProject = projectName == DataStorageProjectName;
        var isAllowedProject = allowedReferencingProjects.Contains(projectName);

        return isDataStorageProject || isAllowedProject;
    }

    private static List<XElement> GetDataStorageReferences(string projectFile)
    {
        try
        {
            var projectContent = File.ReadAllText(projectFile);
            var doc = XDocument.Parse(projectContent);

            var dataStorageReferences = doc.Descendants(ProjectReferenceElement)
                .Where(pr => pr.Attribute(IncludeAttribute)?.Value.Contains(DataStorageProjectName) == true)
                .ToList();

            return dataStorageReferences;
        }
        catch (XmlException ex)
        {
            Assert.Fail($"Failed to parse project file '{projectFile}': {ex.Message}");
            return new List<XElement>(); // Never reached but satisfies compiler
        }
    }

    private static void AddProjectReferenceViolations(
        string projectFile,
        List<XElement> dataStorageReferences,
        List<(string ProjectPath, string ReferenceLine)> violations)
    {
        foreach (var reference in dataStorageReferences)
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

        var sourceFiles = GetSourceFilesExcludingProgramCs(projectDirectory);

        foreach (var sourceFile in sourceFiles)
        {
            CheckFileForDataStorageImports(sourceFile, violations);
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

    private static List<string> GetSourceFilesExcludingProgramCs(string projectDirectory)
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

    private static void CheckFileForDataStorageImports(
        string sourceFile,
        List<(string ProjectPath, string ReferenceLine)> violations)
    {
        var lines = File.ReadLines(sourceFile);
        var lineNumber = 0;

        foreach (var line in lines)
        {
            lineNumber++;
            var trimmedLine = line.Trim();

            if (IsDataStorageNamespaceImport(trimmedLine))
            {
                var violationMessage = $"Line {lineNumber}: {trimmedLine} - Direct DataStorage namespace import not allowed";
                violations.Add((sourceFile, violationMessage));
            }
        }
    }

    private static bool IsDataStorageNamespaceImport(string line)
    {
        var isExactMatch = line.StartsWith(DataStorageNamespaceExact);
        var isSubNamespace = line.StartsWith(DataStorageNamespacePrefix);

        var isViolation = isExactMatch || isSubNamespace;
        return isViolation;
    }

    private static string BuildErrorMessage(List<(string ProjectPath, string ReferenceLine)> violations)
    {
        var violationsSummary = FormatViolations(violations);
        var errorMessage = GetArchitecturalViolationMessage(violationsSummary);

        return errorMessage;
    }

    private static string FormatViolations(List<(string ProjectPath, string ReferenceLine)> violations)
    {
        var formattedViolations = violations.Select(v => $"  File: {v.ProjectPath}\n  Issue: {v.ReferenceLine}");
        var violationsSummary = string.Join("\n\n", formattedViolations);

        return violationsSummary;
    }

    private static string GetArchitecturalViolationMessage(string violationsSummary)
    {
        var message = $@"ARCHITECTURAL VIOLATION: DataStorage is accessed incorrectly:

{violationsSummary}

DataStorage should ONLY be accessed through its gRPC client interface.

HOW TO FIX THIS:

For unauthorized project references:
1. Remove the direct ProjectReference to DataStorage from the violating project(s)
2. Add a reference to DataStorageGrpcClient instead:
   <ProjectReference Include=""..\DataStorageGrpcClient\DataStorageGrpcClient.csproj"" />

For direct usage:
1. Do NOT import DataStorage namespace in source files (except for IDbConnectionFactory usage in Program.cs)
2. Remove any 'using DataStorage;' statements from controllers/services
3. ONLY inject repository interfaces (ILoanRepository, IBorrowingPatternRepository, IBookRepository from DataStorageContracts)
4. The interfaces are automatically resolved to the gRPC clients via dependency injection

SETUP IN PROGRAM.CS:
  using DataStorageGrpcClient.Setup;

  var grpcServerAddress = builder.Configuration[""GrpcServer:Address""] ?? ""http://localhost:5001"";

  // Register the gRPC client (replaces AddDataStorageServices)
  builder.Services.AddDataStorageGrpcClient(grpcServerAddress);

  // Map the gRPC service endpoints
  app.MapDataStorageGrpcServices();

USAGE IN BUSINESS LOGIC OR CONTROLLERS:
  using DataStorageContracts; // Use contracts, NOT DataStorage

  public class MyService
  {{
      private readonly ILoanRepository _loanRepository; // Use interface, NOT concrete class

      public MyService(ILoanRepository loanRepository)
      {{
          _loanRepository = loanRepository;
      }}
  }}

ALLOWED DIRECT REFERENCES:
Direct references to DataStorage.csproj are allowed for hosting the gRPC server. Allowed projects:
  -"+ string.Join(Environment.NewLine + "  -", GetAllowedReferencingProjectsForDi())+
                      $@"

To allow a new project to reference DataStorage (e.g., for hosting gRPC server):
  - Add the project to GetAllowedReferencingProjectsForDi() in DataStorageArchitectureTests
  - Use DataStorageGrpcClient.Setup extension methods (AddDataStorageGrpcClient, MapDataStorageGrpcServices)
  - Do NOT import 'using DataStorage;' in source files (except for IDbConnectionFactory in Program.cs)

This architectural constraint ensures:
  - Proper separation of concerns
  - Scalability through microservices architecture
  - Ability to deploy DataStorage independently
  - Type-safe inter-service communication through gRPC";

        return message;
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
}
