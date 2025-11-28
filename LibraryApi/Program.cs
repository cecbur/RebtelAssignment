using BusinessLogic;
using DataStorage;
using DataStorage.Repositories;
using DataStorage.RepositoriesMultipleTables;
using DataStorage.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on multiple ports
builder.WebHost.ConfigureKestrel(options =>
{
    // HTTP/1.1 endpoint for the web API
    options.ListenLocalhost(7000, o => o.Protocols = HttpProtocols.Http1);
    // HTTP/2 endpoint for gRPC
    options.ListenLocalhost(5001, o => o.Protocols = HttpProtocols.Http2);
});

// Add services to the container
builder.Services.AddControllers();

// Add gRPC services
builder.Services.AddGrpc();

// Configure database connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=Library;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

// Register Dapper connection factory - Dependency Injection (SOLID: Dependency Inversion Principle)
builder.Services.AddSingleton<IDbConnectionFactory>(sp => new SqlServerConnectionFactory(connectionString));

// Register application services - Dependency Injection (SOLID: Dependency Inversion Principle)
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IPatronRepository, PatronRepository>();

// Register business logic services
builder.Services.AddScoped<UserActivity>();
builder.Services.AddScoped<BorrowingPatterns>();
builder.Services.AddScoped<BookPatterns>();

// Register DataStorage LoanRepository as concrete type for gRPC service to inject
builder.Services.AddScoped<LoanRepository>();

// Register DataStorage BorrowingPatternRepository as concrete type for gRPC service to inject
builder.Services.AddScoped<BorrowingPatternRepository>();

// Register gRPC server address
var grpcServerAddress = builder.Configuration["GrpcServer:Address"] ?? "http://localhost:5001";

// Register DataStorageClient LoanRepository for controllers to use (via gRPC)
builder.Services.AddScoped<DataStorageContracts.ILoanRepository>(sp =>
    new DataStorageGrpcClient.LoanRepository(grpcServerAddress));

// Register DataStorageClient BorrowingPatternRepository for controllers to use (via gRPC)
builder.Services.AddScoped<DataStorageContracts.IBorrowingPatternRepository>(sp =>
    new DataStorageGrpcClient.BorrowingPatternRepository(grpcServerAddress));

// Configure Swagger/OpenAPI for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Library API",
        Version = "v1",
        Description = "A well-structured .NET 8 API for library management following Clean Code and SOLID principles",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Library API"
        }
    });

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
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Library API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Map gRPC services
app.MapGrpcService<LoanGrpcService>();
app.MapGrpcService<BorrowingPatternGrpcService>();

// Log startup information
app.Logger.LogInformation("Library API started successfully");
app.Logger.LogInformation("Swagger UI available at: https://localhost:{Port}",
    app.Configuration["ASPNETCORE_HTTPS_PORT"] ?? "7000");
app.Logger.LogInformation("gRPC service available at: http://localhost:5001");

app.Run();
