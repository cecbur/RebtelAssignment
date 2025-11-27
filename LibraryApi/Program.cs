using BusinessLogic;
using DataStorage;
using DataStorage.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure database connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=Library;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

// Register Dapper connection factory - Dependency Injection (SOLID: Dependency Inversion Principle)
builder.Services.AddSingleton<IDbConnectionFactory>(sp => new SqlConnectionFactory(connectionString));

// Register application services - Dependency Injection (SOLID: Dependency Inversion Principle)
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IBookRepository, BookRepository>();

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

// Log startup information
app.Logger.LogInformation("Library API started successfully");
app.Logger.LogInformation("Swagger UI available at: https://localhost:{Port}",
    app.Configuration["ASPNETCORE_HTTPS_PORT"] ?? "7000");

app.Run();
