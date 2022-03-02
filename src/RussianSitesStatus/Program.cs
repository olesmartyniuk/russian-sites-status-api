using RussianSitesStatus.Services;
using RussianSitesStatus.Models;
using RussianSitesStatus.BackgroundServices;
using RussianSitesStatus.Services.Contracts;
using RussianSitesStatus.Configuration;
using System.Reflection;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<StatusCakeService>();
builder.Services.AddSingleton<Storage<Site>>();
builder.Services.AddSingleton<Storage<SiteDetails>>();


builder.Services.AddSingleton<SyncSitesService>();
builder.Services.AddSingleton<ISiteSource, IncourseTradeSiteSource>();

builder.Services.AddHostedService<StatusFetcherBackgroundService>();
builder.Services.AddHostedService<SyncSitesBackgroundService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

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
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Zbyrach API");
    c.RoutePrefix = string.Empty;
});
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
