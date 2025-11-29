using System.Reflection;
using System.Xml.Linq;

namespace BusinessLogic.Tests.Architecture;

public class BusinessLogicArchitectureTests
{
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
        // Arrange
        const string businessLogicProjectName = $"{nameof(BusinessLogic)}.csproj";
        var allowedReferencingProjects = new[]
        {
            "BusinessLogic.Tests.csproj", // Only tests are allowed to directly reference BusinessLogic
            "LibraryApi.csproj" // Composition root - sets up DI for both server and client
        };

        // Act - Part 1: Check for unauthorized project references
        var projectFiles = Directory.GetFiles(_solutionDirectory!, "*.csproj", SearchOption.AllDirectories);
        var violations = new List<(string ProjectPath, string ReferenceLine)>();

        foreach (var projectFile in projectFiles)
        {
            var projectName = Path.GetFileName(projectFile);

            // Skip the BusinessLogic project itself and allowed projects
            if (projectName == businessLogicProjectName || allowedReferencingProjects.Contains(projectName))
                continue;

            var projectContent = File.ReadAllText(projectFile);
            var doc = XDocument.Parse(projectContent);

            var businessLogicReferences = doc.Descendants("ProjectReference")
                .Where(pr => pr.Attribute("Include")?.Value.Contains(businessLogicProjectName) == true)
                .ToList();

            if (businessLogicReferences.Any())
            {
                foreach (var reference in businessLogicReferences)
                {
                    var referenceLine = reference.ToString();
                    violations.Add((projectFile, referenceLine));
                }
            }
        }

        // Act - Part 2: Verify LibraryApi only uses BusinessLogic for DI setup
        var libraryApiDir = Path.Combine(_solutionDirectory!, "LibraryApi");
        if (Directory.Exists(libraryApiDir))
        {
            var libraryApiSourceFiles = Directory.GetFiles(libraryApiDir, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Contains("\\bin\\") && !f.Contains("\\obj\\"))
                .Where(f => !f.EndsWith("Program.cs")) // Program.cs is allowed to use BusinessLogic for DI
                .ToList();

            var forbiddenTypes = new[] { "PatronActivity", "BorrowingPatterns", "BookPatterns", "Facade" };

            foreach (var sourceFile in libraryApiSourceFiles)
            {
                var content = File.ReadAllText(sourceFile);
                var lines = content.Split('\n');

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    // Skip comments and using statements
                    if (line.TrimStart().StartsWith("//") || line.TrimStart().StartsWith("using"))
                        continue;

                    foreach (var forbiddenType in forbiddenTypes)
                    {
                        // Check for direct instantiation or usage of BusinessLogic classes
                        if (line.Contains($"new {forbiddenType}") ||
                            line.Contains($"<{forbiddenType}>") ||
                            line.Contains($"({forbiddenType} ") ||
                            (line.Contains($" {forbiddenType} ") && !line.Contains("//") && line.Contains("=")))
                        {
                            violations.Add((sourceFile, $"Line {i + 1}: {line.Trim()}"));
                        }
                    }
                }
            }
        }

        // Assert
        Assert.That(violations, Is.Empty,
            $@"ARCHITECTURAL VIOLATION: BusinessLogic is accessed incorrectly:

{string.Join("\n\n", violations.Select(v => $"  File: {v.ProjectPath}\n  Issue: {v.ReferenceLine}"))}

BusinessLogic should ONLY be accessed through its gRPC client interface.

HOW TO FIX THIS:

For unauthorized project references:
1. Remove the direct ProjectReference to BusinessLogic from the violating project(s)
2. Add a reference to BusinessLogicGrpcClient instead:
   <ProjectReference Include=""..\BusinessLogicGrpcClient\BusinessLogicGrpcClient.csproj"" />

For direct usage in LibraryApi (e.g., controllers):
1. Do NOT inject concrete BusinessLogic classes (PatronActivity, BorrowingPatterns, BookPatterns, Facade)
2. ONLY inject IBusinessLogicFacade (the interface from BusinessLogicContracts)
3. The interface is automatically resolved to the gRPC client via dependency injection

EXAMPLE (in a Controller):
  public class MyController : ControllerBase
  {{
      private readonly IBusinessLogicFacade _businessLogic; // Use interface, NOT concrete class

      public MyController(IBusinessLogicFacade businessLogic)
      {{
          _businessLogic = businessLogic;
      }}
  }}

ALLOWED DIRECT REFERENCES:
  - BusinessLogic.Tests.csproj (for unit testing)
  - LibraryApi.csproj (for DI setup in Program.cs only)

This architectural constraint ensures:
  - Proper separation of concerns
  - Scalability through microservices architecture
  - Ability to deploy BusinessLogic independently
  - Type-safe inter-service communication through gRPC");
    }

    [Test]
    public void BusinessLogic_AllOtherProjectsShouldUseGrpcClientOrContracts()
    {
        // Arrange
        var solutionProjects = Directory.GetFiles(_solutionDirectory!, "*.csproj", SearchOption.AllDirectories)
            .Select(Path.GetFileName)
            .Where(name => name != "BusinessLogic.csproj"
                        && name != "BusinessLogic.Tests.csproj"
                        && name != "BusinessLogicGrpcClient.csproj"
                        && name != "BusinessLogicContracts.csproj"
                        && name != "BusinessModels.csproj"
                        && name != "DataStorage.csproj"
                        && name != "DataStorageContracts.csproj"
                        && name != "DataStorageGrpcClient.csproj")
            .ToList();

        var projectsWithoutGrpcAccess = new List<string>();

        // Act
        foreach (var projectName in solutionProjects)
        {
            var projectFile = Directory.GetFiles(_solutionDirectory!, projectName!, SearchOption.AllDirectories).First();
            var projectContent = File.ReadAllText(projectFile);
            var doc = XDocument.Parse(projectContent);

            var hasGrpcClientReference = doc.Descendants("ProjectReference")
                .Any(pr => pr.Attribute("Include")?.Value.Contains("BusinessLogicGrpcClient.csproj") == true);

            var hasContractsReference = doc.Descendants("ProjectReference")
                .Any(pr => pr.Attribute("Include")?.Value.Contains("BusinessLogicContracts.csproj") == true);

            if (!hasGrpcClientReference && !hasContractsReference)
            {
                projectsWithoutGrpcAccess.Add(projectName!);
            }
        }

        // Assert with informative message
        if (projectsWithoutGrpcAccess.Any())
        {
            Assert.Warn(
                $@"ARCHITECTURAL RECOMMENDATION: The following project(s) don't reference BusinessLogicGrpcClient or BusinessLogicContracts:

{string.Join("\n", projectsWithoutGrpcAccess.Select(p => $"  - {p}"))}

If these projects need to access BusinessLogic functionality, they should use:
  - BusinessLogicGrpcClient (for making gRPC calls)
  - BusinessLogicContracts (for interface definitions)

If these projects legitimately don't need BusinessLogic access, add them to the exclusion list
in the .Where() clause (around line 90) to suppress this warning:
  .Where(name => name != ""YourProject.csproj""
              && ...)

This is a warning, not a failure, in case these projects legitimately don't need BusinessLogic access.");
        }
    }
    
    private static string? TryGetSolutionDirectory()
    {
        // Start from the test assembly location
        var assemblyPath = Assembly.GetExecutingAssembly().Location;
        var directory = Path.GetDirectoryName(assemblyPath);

        // Walk up the directory tree looking for a .sln file
        while (directory != null)
        {
            if (Directory.GetFiles(directory, "*.sln").Any())
            {
                return directory;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        return null;
    }
    
}
