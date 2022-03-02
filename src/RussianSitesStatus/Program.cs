using RussianSitesStatus.Services;
using RussianSitesStatus.Models;
using RussianSitesStatus.BackgroundServices;
using RussianSitesStatus.Services.Contracts;
using RussianSitesStatus.Configuration;
using Microsoft.AspNetCore.Authentication;
using RussianSitesStatus.Auth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
RegisteServices(builder.Services);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services
    .AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthHandler>(Scheme.ApiKeyAuthScheme, _ => { });

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
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();


static void RegisteServices(IServiceCollection services)
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
