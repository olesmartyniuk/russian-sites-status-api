using RussianSitesStatus.Services;
using RussianSitesStatus.Models;
using RussianSitesStatus.BackgroundServices;
using RussianSitesStatus.Services.Contracts;
using RussianSitesStatus.Configuration;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication;
using RussianSitesStatus.Auth;

var builder = WebApplication.CreateBuilder(args);

AddService(builder.Services);
AddControllers(builder);
AddSwagger(builder.Services);
AddAuthentication(builder);
AddCors(builder.Services);

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
app.Run();


static void AddService(IServiceCollection services)
{
    services.AddSingleton<StatusCakeService>();
    services.AddSingleton<Storage<Site>>();
    services.AddSingleton<Storage<SiteDetails>>();

    services.AddSingleton<UpCheckService>();
    services.AddSingleton<SyncSitesService>();
    services.AddSingleton<ISiteSource, IncourseTradeSiteSource>();

    services.AddHostedService<StatusFetcherBackgroundService>();
    services.AddHostedService<SyncSitesBackgroundService>();
}

static void AddCors(IServiceCollection services)
{
    services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy",
            builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
    });
}

static void AddSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(c =>
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