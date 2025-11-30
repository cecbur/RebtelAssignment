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

        var errorMessage = BuildErrorMessage(violations);
        Assert.That(violations, Is.Empty, errorMessage);
    }

    /// <summary>
    /// Returns the list of projects allowed to have a ProjectReference to BusinessLogic.csproj.
    /// These projects are typically composition roots that sets up DI for both server and client.
    /// They must use BusinessLogicGrpcClient.Setup extension methods and should not import the
    /// BusinessLogic namespace in source files.
    /// </summary>
    /// <returns>Array of .csproj file names that are allowed to reference BusinessLogic</returns>
    protected override string[] GetAllowedReferencingProjectsForDi()
    {
        return
        [
            "LibraryApi.csproj", // Composition root - sets up DI for both server and client
            "LibraryApiTests.csproj"
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
            "ArchitectureTests.csproj",
        ];
    }

    protected override string GetSetupMethodName() => "AddBusinessLogicGrpcClient";

    protected override string GetMapMethodName() => "MapBusinessLogicGrpcService";

    protected override string GetInterfaceInstructions() =>
        "ONLY inject IBusinessLogicFacade (the interface from BusinessLogicContracts)";

    protected override bool UsePluralInterfaces() => false;

    protected override bool UsesPluralServices() => false;

    protected override string GetUsageExample() => @"USAGE IN CONTROLLERS:
  using BusinessLogicContracts.Interfaces; // Use contracts, NOT BusinessLogic

  public class MyController : ControllerBase
  {
      private readonly IBusinessLogicFacade _businessLogic; // Use interface, NOT concrete class

      public MyController(IBusinessLogicFacade businessLogic)
      {
          _businessLogic = businessLogic;
      }
  }";

    protected override string GetAdditionalSetupNotes() => "";
}
