using RussianSitesStatus.Services;
using RussianSitesStatus.Models;
using RussianSitesStatus.BackgroundServices;
using RussianSitesStatus.Services.Contracts;
using RussianSitesStatus.Configuration;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication;
using RussianSitesStatus.Auth;
using RussianSitesStatus.Database;
using Microsoft.EntityFrameworkCore;
using RussianSitesStatus.Services.StatusCake;

var builder = WebApplication.CreateBuilder(args);

AddServices(builder);
AddControllers(builder);
AddSwagger(builder);
AddAuthentication(builder);
AddCors(builder);

builder.Services.Configure<SyncSitesConfiguration>(builder.Configuration.GetSection(nameof(SyncSitesConfiguration)));

builder.WebHost.UseKestrel((context, options) =>
{
    var port = Environment.GetEnvironmentVariable("PORT");
    if (!string.IsNullOrEmpty(port))
    {
        options.ListenAnyIP(int.Parse(port));
    }
});

// Configure the HTTP request pipeline.
var app = builder.Build();
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
            logger.LogInformation("There are no pending migrations");
        }
        context.Database.Migrate();
        logger.LogInformation("The database has been successfully migrated.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the DB.");
    }
}


static void AddServices(WebApplicationBuilder builder)
{
    var services = builder.Services;

    services.AddDbContext<ApplicationContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString());
    });

    services.AddSingleton<StatusCakeService>();
    services.AddSingleton<InMemoryStorage<SiteVM>>();
    services.AddSingleton<InMemoryStorage<SiteDetailsVM>>();

    services.AddSingleton<StatusCakeUpCheckService>();
    services.AddSingleton<ISyncSitesService, SyncStatusCakeSitesService>();

    services.AddSingleton<ISiteSource, IncourseTradeSiteSource>();

    services.AddScoped<DatabaseStorage>();

    services.AddSingleton<IFetchDataService, StatusCakeFetchDataService>();

    services.AddHostedService<MemoryDataFetcher>();
    services.AddHostedService<SyncSitesWorker>();
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