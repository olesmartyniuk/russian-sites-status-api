using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.OpenApi.Models;
using RussianSitesStatus.Auth;
using RussianSitesStatus.BackgroundServices;
using RussianSitesStatus.Database;
using RussianSitesStatus.Models;
using RussianSitesStatus.Services;
using RussianSitesStatus.Services.Contracts;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

AddServices(builder);
AddSwagger(builder);
AddControllers(builder);
AddAuthentication(builder);
AddCors(builder);

ConfigureKestrel(builder);

// Configure the HTTP request pipeline.
var app = builder.Build();

ConfigureHttpPipeline(app);

CreateDbIfNotExist(app);

app.Run();


static void CreateDbIfNotExist(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<ApplicationContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        var migrations = context.Database.GetPendingMigrations().ToList();
        if (migrations.Any())
        {
            logger.LogInformation("Service is going to run migrations: {migrations}.", string.Join(", ", migrations));
        }
        else
        {
            logger.LogTrace("There are no pending migrations");
        }
        context.Database.Migrate();
        logger.LogTrace("The database has been successfully migrated.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the DB.");
    }
}


static void AddServices(WebApplicationBuilder builder)
{
    builder.Host.UseSystemd();

    var services = builder.Services;

    services.AddDbContext<ApplicationContext>(options =>
    {
        options.ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuting, LogLevel.Trace)));
        options.ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuted, LogLevel.Trace)));
        options.ConfigureWarnings(c => c.Log((RelationalEventId.ConnectionOpening, LogLevel.Trace)));
        options.ConfigureWarnings(c => c.Log((RelationalEventId.ConnectionOpened, LogLevel.Trace)));
        options.ConfigureWarnings(c => c.Log((RelationalEventId.ConnectionClosing, LogLevel.Trace)));
        options.ConfigureWarnings(c => c.Log((RelationalEventId.ConnectionClosed, LogLevel.Trace)));

        options.UseNpgsql(builder.Configuration.GetConnectionString(),
                optionsAction =>
                {
                    optionsAction.EnableRetryOnFailure(2, TimeSpan.FromSeconds(5), null);
                }
            );
    }, ServiceLifetime.Scoped);
    
    services.AddSingleton<SiteStorage>();
    services.AddSingleton<RegionStorage>();
    services.AddSingleton<StatisticStorage>();

    services.AddSingleton<ISiteSource, IncourseTradeSiteSource>();
    services.AddSingleton<IFetchDataService, FetchDataService>();
    services.AddTransient<MonitorSitesStatusService>();
    services.AddTransient<ICheckSiteService, CheckSiteService>();
    services.AddSingleton<CleanupChecksService>();

    services.AddScoped<DatabaseStorage>();
    services.AddTransient<ISyncSitesService, SyncSitesDatabaseService>();
    services.AddSingleton<ArchiveStatisticService>();

    services.AddHostedService<MemoryDataFetcher>();
    services.AddHostedService<SyncSitesWorker>();
    services.AddHostedService<MonitorStatusWorker>();
    services.AddHostedService<ArchiveStatisticWorker>();
    services.AddHostedService<StatisticDataFetcher>();
    services.AddHostedService<CleanupChecksWorker>();

    builder.Services.AddResponseCompression(options =>
    {
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();
    });
}

static void AddCors(WebApplicationBuilder builder)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy",
            builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
    });
}

static void AddSwagger(WebApplicationBuilder builder)
{
    builder.Services.AddSwaggerGen(c =>
    {
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);

        c.SwaggerDoc("v1",
            new OpenApiInfo
            {
                Title = "Mordor sites status API",
                Version = "",
                Description = @"This service monitors popular Russian and Belarusian sites and checks their availability. 
                The checks are performed from different parts of the world via HTTP with a frequency of 30 seconds. 
                One of the monitoring servers is located in the territory of the Russian Federation.
                The list of sites is taken from our friends Help in DDOS attacks (https://incourse.trade/sites.php).",
            });
    });
}

static void AddAuthentication(WebApplicationBuilder builder)
{
    builder.Services
        .AddAuthentication()
        .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthHandler>(Scheme.ApiKeyAuthScheme, _ => { });
}

static void AddControllers(WebApplicationBuilder builder)
{
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
}

void ConfigureKestrel(WebApplicationBuilder builder)
{
    builder.WebHost.UseKestrel();
}

void ConfigureHttpPipeline(WebApplication app)
{
    app.UseResponseCompression();
    app.UseCors("CorsPolicy");
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mordor sites status API");
        c.RoutePrefix = string.Empty;
        c.DocumentTitle = "Mordor sites status API";
    });
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();
}