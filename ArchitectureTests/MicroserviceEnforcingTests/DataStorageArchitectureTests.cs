namespace ArchitectureTests.MicroserviceEnforcingTests;

public class DataStorageArchitectureTests : MicroserviceArchitectureTestBase
{
    private const string DataStorageProjectName = "DataStorage.csproj";
    private const string DataStorageGrpcClientName = "DataStorageGrpcClient.csproj";
    private const string DataStorageNamespaceExact = "using DataStorage;";
    private const string DataStorageNamespacePrefix = "using DataStorage.";

    [Test]
    public void DataStorage_ShouldOnlyBeAccessedThroughGrpcClient()
    {
        var allowedReferencingProjects = GetAllowedReferencingProjectsForDi();
        var violations = CollectAllViolations(allowedReferencingProjects);

        var errorMessage = BuildErrorMessage(violations, GetArchitecturalViolationMessage(allowedReferencingProjects));
        Assert.That(violations, Is.Empty, errorMessage);
    }

    /// <summary>
    /// Returns the list of projects allowed to have a ProjectReference to DataStorage.csproj.
    /// These projects are typically composition roots that set up DI for both server and client.
    /// They must use DataStorageGrpcClient.Setup extension methods and should not import the DataStorage namespace in source files (except for IDbConnectionFactory usage).
    /// </summary>
    /// <returns>Array of .csproj file names that are allowed to reference DataStorage</returns>
    protected override string[] GetAllowedReferencingProjectsForDi()
    {
        return
        [
            "LibraryApi.csproj", // Composition root - sets up DI for both server and client
        ];
    }

    protected override string GetMicroserviceProjectName() => DataStorageProjectName;

    protected override string GetMicroserviceGrpcClientName() => DataStorageGrpcClientName;

    protected override string GetMicroserviceNamespaceExact() => DataStorageNamespaceExact;

    protected override string GetMicroserviceNamespacePrefix() => DataStorageNamespacePrefix;

    protected override string GetMicroserviceName() => "DataStorage";

    protected override string[] GetProjectsToExcludeFromScanning()
    {
        return
        [
            DataStorageProjectName,
            DataStorageGrpcClientName,
            ArchitectureTestsProjectName
        ];
    }

    protected override string GetArchitecturalViolationMessage(string[] allowedProjects)
    {
        var message = $@"ARCHITECTURAL VIOLATION: DataStorage is accessed incorrectly:

{{violations}}

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
  -" + string.Join(Environment.NewLine + "  -", allowedProjects) +
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
}
