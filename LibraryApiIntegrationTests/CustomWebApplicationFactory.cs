using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestData;

namespace LibraryApiIntegrationTests;

/// <summary>
/// Custom WebApplicationFactory that configures the LibraryApi to use the test database.
/// NOTE: This is kept for reference but not currently used due to gRPC complexity.
/// See GetBooksSortedByMostLoanedTests for the actual test implementation.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override the connection string to use the test database
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = SqlServerTestFixture.ConnectionString
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove and re-register the IDbConnectionFactory to use the test connection string
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DataStorage.IDbConnectionFactory));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddSingleton<DataStorage.IDbConnectionFactory>(
                sp => new DataStorage.SqlServerConnectionFactory(SqlServerTestFixture.ConnectionString));
        });
    }
}
