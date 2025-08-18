using Api.Infrastructure.Data;
using Api.Infrastructure.Persistence;
using Api.Infrastructure.Seed;
using Api.Infrastructure.Storage;
using Api.Middleware;
using Api.Models;
using Api.Services;
using Api.Services.Export;
using Api.Services.Nlp;
using Api.Services.Processing;
using Api.Validation;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

// ---- EF Core: Use In-Memory for development ----
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<ResearchDbContext>(opts =>
    {
        opts.UseInMemoryDatabase("ResearchDb");
    });
}
else
{
    var conn = builder.Configuration.GetConnectionString("Default");
    if (string.IsNullOrWhiteSpace(conn))
    {
        throw new InvalidOperationException("Missing ConnectionStrings:Default. Add it to appsettings.Development.json.");
    }
    builder.Services.AddDbContext<ResearchDbContext>(opts =>
    {
        opts.UseSqlServer(conn);
    });
}

// ---- Web basics ----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Research Paper Summarizer API", Version = "v1" });
    
    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<UploadValidator>();

// Hangfire with In-Memory for development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHangfire(config =>
        config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
              .UseSimpleAssemblyNameTypeSerializer()
              .UseRecommendedSerializerSettings()
              .UseMemoryStorage());
}
else
{
    var conn = builder.Configuration.GetConnectionString("Default");
    builder.Services.AddHangfire(config =>
        config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
              .UseSimpleAssemblyNameTypeSerializer()
              .UseRecommendedSerializerSettings()
              .UseSqlServerStorage(conn, new SqlServerStorageOptions
              {
                  CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                  SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                  QueuePollInterval = TimeSpan.Zero,
                  UseRecommendedIsolationLevel = true,
                  DisableGlobalLocks = true
              }));
}

builder.Services.AddHangfireServer();

// HTTP Client for NLP service
builder.Services.AddHttpClient<INlpClient, NlpClient>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// (Optional) CORS if your frontend is localhost:3000
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
{
    p.WithOrigins(builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" })
     .AllowAnyHeader()
     .AllowAnyMethod();
}));

// Configuration options
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));
builder.Services.Configure<NlpOptions>(builder.Configuration.GetSection("Nlp"));
builder.Services.Configure<LimitsOptions>(builder.Configuration.GetSection("Limits"));

// Application services
builder.Services.AddScoped<IPaperRepository, PaperRepository>();
builder.Services.AddScoped<IPaperService, PaperService>();
builder.Services.AddScoped<IExporter, ExportService>();
builder.Services.AddScoped<MarkdownExporter>();
builder.Services.AddScoped<PdfExporter>();
builder.Services.AddScoped<ProcessingPipeline>();
builder.Services.AddSingleton<IFileStorage, FileStorage>();

var app = builder.Build();

// ---- Auto-migrate safely (only for relational providers) ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ResearchDbContext>();
    try
    {
        if (db.Database.IsRelational())
        {
            await db.Database.MigrateAsync();
        }
        else
        {
            // If someone swaps to InMemory for tests, don't crash
            await db.Database.EnsureCreatedAsync();
        }
        await DbSeeder.SeedAsync(db);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Could not migrate database. Ensure SQL Server is running and connection string is correct.");
        // Continue startup without database - the app will show connection errors when database operations are attempted
    }
}

// ---- Pipeline ----
app.UseSerilogRequestLogging();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Research Paper Summarizer API v1");
        c.RoutePrefix = "swagger";
    });
    
    // Hangfire Dashboard (development only)
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireAuthorizationFilter() }
    });
}

app.MapControllers();
app.MapGet("/healthz", () => new { status = "ok" });

app.Run();

// Hangfire authorization filter for development
public class HangfireAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(Hangfire.Dashboard.DashboardContext context)
    {
        // Allow access in development environment
        return true;
    }
}