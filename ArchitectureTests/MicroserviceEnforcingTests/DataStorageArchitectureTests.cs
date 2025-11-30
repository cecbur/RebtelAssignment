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

        var errorMessage = BuildErrorMessage(violations);
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

    protected override string GetSetupMethodName() => "AddDataStorageGrpcClient";

    protected override string GetMapMethodName() => "MapDataStorageGrpcServices";

    protected override string GetInterfaceInstructions() =>
        "ONLY inject repository interfaces (ILoanRepository, IBorrowingPatternRepository, IBookRepository from DataStorageContracts)";

    protected override bool UsePluralInterfaces() => true;

    protected override bool UsesPluralServices() => true;

    protected override string GetUsageExample() => @"USAGE IN BUSINESS LOGIC OR CONTROLLERS:
  using DataStorageContracts; // Use contracts, NOT DataStorage

  public class MyService
  {
      private readonly ILoanRepository _loanRepository; // Use interface, NOT concrete class

      public MyService(ILoanRepository loanRepository)
      {
          _loanRepository = loanRepository;
      }
  }";

    protected override string GetAdditionalSetupNotes() => " (except for IDbConnectionFactory usage in Program.cs)";
}
