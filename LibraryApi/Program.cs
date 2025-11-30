using BusinessLogicGrpcClient.Setup;
using DataStorageGrpcClient.Setup;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on multiple ports
// Allow ports to be configured via environment variables for testing
var httpPort = int.TryParse(builder.Configuration["Kestrel:HttpPort"], out var hp) ? hp : 7000;
var grpcPort = int.TryParse(builder.Configuration["Kestrel:GrpcPort"], out var gp) ? gp : 5001;

builder.WebHost.ConfigureKestrel(options =>
{
    // HTTP/1.1 endpoint for the web API
    options.ListenLocalhost(httpPort, o => o.Protocols = HttpProtocols.Http1);
    // HTTP/2 endpoint for gRPC
    options.ListenLocalhost(grpcPort, o => o.Protocols = HttpProtocols.Http2);
});

// Add services to the container
builder.Services.AddControllers();

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new Asp.Versioning.UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

// Add gRPC services
builder.Services.AddGrpc();

// Configure database connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=Library;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

// Register Dapper connection factory - Dependency Injection (SOLID: Dependency Inversion Principle)
builder.Services.AddSingleton<DataStorage.IDbConnectionFactory>(sp => new DataStorage.SqlServerConnectionFactory(connectionString));

// Register gRPC server address
var grpcServerAddress = builder.Configuration["GrpcServer:Address"] ?? "http://localhost:5001";

// Register DataStorage and BusinessLogic gRPC clients
builder.Services.AddDataStorageGrpcClient(grpcServerAddress);
builder.Services.AddBusinessLogicGrpcClient(grpcServerAddress);

// Register commands
builder.Services.AddScoped<LibraryApi.Commands.AssignmentCommands.GetBooksSortedByMostLoanedCommand>();
builder.Services.AddScoped<LibraryApi.Commands.AssignmentCommands.GetMostActivePatronsCommand>();
builder.Services.AddScoped<LibraryApi.Commands.AssignmentCommands.GetReadingPacePagesPerDayCommand>();
builder.Services.AddScoped<LibraryApi.Commands.AssignmentCommands.GetOtherBooksBorrowedCommand>();

// Configure Swagger/OpenAPI for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Generate a Swagger document for each discovered API version
    var provider = builder.Services.BuildServiceProvider().GetRequiredService<Asp.Versioning.ApiExplorer.IApiVersionDescriptionProvider>();

    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerDoc(description.GroupName, new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Library API",
            Version = description.ApiVersion.ToString(),
            Description = description.IsDeprecated
                ? "A well-structured .NET 8 API for library management following Clean Code and SOLID principles (DEPRECATED)"
                : "A well-structured .NET 8 API for library management following Clean Code and SOLID principles",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "Library API"
            }
        });
    }

    // Enable XML comments for better API documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Add CORS support (useful for frontend integration)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<Asp.Versioning.ApiExplorer.IApiVersionDescriptionProvider>();
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"Library API {description.GroupName.ToUpperInvariant()}");
        }
        options.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Map gRPC services
app.MapDataStorageGrpcServices();
app.MapBusinessLogicGrpcService();

// Log startup information
app.Logger.LogInformation("Library API started successfully");
app.Logger.LogInformation("HTTP API available at: http://localhost:{Port}", httpPort);
app.Logger.LogInformation("gRPC service available at: http://localhost:{Port}", grpcPort);

app.Run();
