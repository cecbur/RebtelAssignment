namespace ArchitectureTests.MicroserviceEnforcingTests;

public class BusinessLogicArchitectureTests : MicroserviceArchitectureTestBase
{
    private const string BusinessLogicProjectName = "BusinessLogic.csproj";
    private const string BusinessLogicTestsProjectName = "BusinessLogicTests.csproj";
    private const string BusinessLogicGrpcClientName = "BusinessLogicGrpcClient.csproj";
    private const string BusinessLogicNamespaceExact = "using BusinessLogic;";
    private const string BusinessLogicNamespacePrefix = "using BusinessLogic.";

    [Test]
    public void BusinessLogic_ShouldOnlyBeAccessedThroughGrpcClient()
    {
        var allowedReferencingProjects = GetAllowedReferencingProjectsForDi();
        var violations = CollectAllViolations(allowedReferencingProjects);

        var errorMessage = BuildErrorMessage(violations, GetArchitecturalViolationMessage(allowedReferencingProjects));
        Assert.That(violations, Is.Empty, errorMessage);
    }

    /// <summary>
    /// Returns the list of projects allowed to have a ProjectReference to BusinessLogic.csproj.
    /// These projects are typically composition roots that sets up DI for both server and client.
    /// They must use BusinessLogicGrpcClient.Setup extension methods and should not import the BusinessLogic namespace in source files.
    /// </summary>
    /// <returns>Array of .csproj file names that are allowed to reference BusinessLogic</returns>
    protected override string[] GetAllowedReferencingProjectsForDi()
    {
        return
        [
            "LibraryApi.csproj", // Composition root - sets up DI for both server and client
        ];
    }

    protected override string GetMicroserviceProjectName() => BusinessLogicProjectName;

    protected override string GetMicroserviceGrpcClientName() => BusinessLogicGrpcClientName;

    protected override string GetMicroserviceNamespaceExact() => BusinessLogicNamespaceExact;

    protected override string GetMicroserviceNamespacePrefix() => BusinessLogicNamespacePrefix;

    protected override string GetMicroserviceName() => "BusinessLogic";

    protected override string[] GetProjectsToExcludeFromScanning()
    {
        return
        [
            BusinessLogicProjectName,
            BusinessLogicTestsProjectName,
            BusinessLogicGrpcClientName,
            ArchitectureTestsProjectName
        ];
    }

    protected override string GetArchitecturalViolationMessage(string[] allowedProjects)
    {
        var message = $@"ARCHITECTURAL VIOLATION: BusinessLogic is accessed incorrectly:

{{violations}}

BusinessLogic should ONLY be accessed through its gRPC client interface.

HOW TO FIX THIS:

For unauthorized project references:
1. Remove the direct ProjectReference to BusinessLogic from the violating project(s)
2. Add a reference to BusinessLogicGrpcClient instead:
   <ProjectReference Include=""..\BusinessLogicGrpcClient\BusinessLogicGrpcClient.csproj"" />

For direct usage:
1. Do NOT import BusinessLogic namespace in any source files
2. Remove any 'using BusinessLogic;' statements from controllers/services
3. ONLY inject IBusinessLogicFacade (the interface from BusinessLogicContracts)
4. The interface is automatically resolved to the gRPC client via dependency injection

SETUP IN PROGRAM.CS:
  using BusinessLogicGrpcClient.Setup;

  var grpcServerAddress = builder.Configuration[""GrpcServer:Address""] ?? ""http://localhost:5001"";

  // Register the gRPC client (replaces AddBusinessLogicServices)
  builder.Services.AddBusinessLogicGrpcClient(grpcServerAddress);

  // Map the gRPC service endpoint
  app.MapBusinessLogicGrpcService();

USAGE IN CONTROLLERS:
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
Direct references to BusinessLogic.csproj are allowed for hosting the gRPC server. Allowed projects:
  -" + string.Join(Environment.NewLine + "  -", allowedProjects) +
                      $@"

To allow a new project to reference BusinessLogic (e.g., for hosting gRPC server):
  - Add the project to GetAllowedReferencingProjectsForDi() in BusinessLogicArchitectureTests
  - Use BusinessLogicGrpcClient.Setup extension methods (AddBusinessLogicGrpcClient, MapBusinessLogicGrpcService)
  - Do NOT import 'using BusinessLogic;' in any source files - use extension methods instead

This architectural constraint ensures:
  - Proper separation of concerns
  - Scalability through microservices architecture
  - Ability to deploy BusinessLogic independently
  - Type-safe inter-service communication through gRPC";

        return message;
    }
}
