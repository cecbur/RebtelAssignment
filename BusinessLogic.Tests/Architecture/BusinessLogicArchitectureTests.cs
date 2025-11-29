using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace BusinessLogic.Tests.Architecture;

public class BusinessLogicArchitectureTests
{
    private const string ProjectFilePattern = "*.csproj";
    private const string SourceFilePattern = "*.cs";
    private const string BinDirectory = "bin";
    private const string ObjDirectory = "obj";
    private const string BusinessLogicNamespaceExact = "using BusinessLogic;";
    private const string BusinessLogicNamespacePrefix = "using BusinessLogic.";
    private const string ProjectReferenceElement = "ProjectReference";
    private const string IncludeAttribute = "Include";
    private const string BusinessLogicProjectName = "BusinessLogic.csproj";
    private const string BusinessLogicTestsProjectName = "BusinessLogic.Tests.csproj";
    private const string BusinessLogicGrpcClientName = "BusinessLogicGrpcClient.csproj";

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
    public void BusinessLogic_ShouldOnlyBeAccessedThroughGrpcClient()
    {
        var allowedReferencingProjects = GetAllowedReferencingProjectsForDi();
        var violations = CollectAllViolations(allowedReferencingProjects);

        var errorMessage = BuildErrorMessage(violations);
        Assert.That(violations, Is.Empty, errorMessage);
        
    }

    private static string[] GetAllowedReferencingProjectsForDi()
    {
        return
        [
            "LibraryApi.csproj", // Composition root - sets up DI for both server and client
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
                return filename != BusinessLogicProjectName && 
                       filename != BusinessLogicTestsProjectName &&
                       filename != BusinessLogicGrpcClientName;
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

        var businessLogicReferences = GetBusinessLogicReferences(projectFile);
        AddProjectReferenceViolations(projectFile, businessLogicReferences, violations);
    }

    private static bool ShouldSkipProject(string projectName, string[] allowedReferencingProjects)
    {
        var isBusinessLogicProject = projectName == BusinessLogicProjectName;
        var isAllowedProject = allowedReferencingProjects.Contains(projectName);

        return isBusinessLogicProject || isAllowedProject;
    }

    private static List<XElement> GetBusinessLogicReferences(string projectFile)
    {
        try
        {
            var projectContent = File.ReadAllText(projectFile);
            var doc = XDocument.Parse(projectContent);

            var businessLogicReferences = doc.Descendants(ProjectReferenceElement)
                .Where(pr => pr.Attribute(IncludeAttribute)?.Value.Contains(BusinessLogicProjectName) == true)
                .ToList();

            return businessLogicReferences;
        }
        catch (XmlException ex)
        {
            Assert.Fail($"Failed to parse project file '{projectFile}': {ex.Message}");
            return new List<XElement>(); // Never reached but satisfies compiler
        }
    }

    private static void AddProjectReferenceViolations(
        string projectFile,
        List<XElement> businessLogicReferences,
        List<(string ProjectPath, string ReferenceLine)> violations)
    {
        foreach (var reference in businessLogicReferences)
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
            CheckFileForBusinessLogicImports(sourceFile, violations);
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

    private static void CheckFileForBusinessLogicImports(
        string sourceFile,
        List<(string ProjectPath, string ReferenceLine)> violations)
    {
        var lines = File.ReadLines(sourceFile);
        var lineNumber = 0;

        foreach (var line in lines)
        {
            lineNumber++;
            var trimmedLine = line.Trim();

            if (IsBusinessLogicNamespaceImport(trimmedLine))
            {
                var violationMessage = $"Line {lineNumber}: {trimmedLine} - Direct BusinessLogic namespace import not allowed";
                violations.Add((sourceFile, violationMessage));
            }
        }
    }

    private static bool IsBusinessLogicNamespaceImport(string line)
    {
        var isExactMatch = line.StartsWith(BusinessLogicNamespaceExact);
        var isSubNamespace = line.StartsWith(BusinessLogicNamespacePrefix);

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
        var message = $@"ARCHITECTURAL VIOLATION: BusinessLogic is accessed incorrectly:

{violationsSummary}

BusinessLogic should ONLY be accessed through its gRPC client interface.

HOW TO FIX THIS:

For unauthorized project references:
1. Remove the direct ProjectReference to BusinessLogic from the violating project(s)
2. Add a reference to BusinessLogicGrpcClient instead:
   <ProjectReference Include=""..\BusinessLogicGrpcClient\BusinessLogicGrpcClient.csproj"" />

For direct usage :
1. Do NOT import BusinessLogic namespace in non-Program.cs files
2. Remove any 'using BusinessLogic;' statements from controllers/services
3. ONLY inject IBusinessLogicFacade (the interface from BusinessLogicContracts)
4. The interface is automatically resolved to the gRPC client via dependency injection

EXAMPLE (in a Controller):
  using BusinessLogicContracts.Interfaces; // Use contracts, NOT BusinessLogic

  public class MyController : ControllerBase
  {{
      private readonly IBusinessLogicFacade _businessLogic; // Use interface, NOT concrete class

      public MyController(IBusinessLogicFacade businessLogic)
      {{
          _businessLogic = businessLogic;
      }}
  }}

ALLOWED DIRECT REFERENCES:
Direct references are allowed for testing and dependency injection Program.cs only. Allowed projects are
  -"+ string.Join(Environment.NewLine + "  -", GetAllowedReferencingProjectsForDi())+
                      $@"

To allow a new project to reference BusinessLogic (e.g., for DI setup):
  - Add the project to the 'allowedReferencingProjects' array in this test (around line 25)
  - Ensure the project only uses BusinessLogic in Program.cs for dependency injection setup
  - All other files in the project must use BusinessLogicGrpcClient instead

This architectural constraint ensures:
  - Proper separation of concerns
  - Scalability through microservices architecture
  - Ability to deploy BusinessLogic independently
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
