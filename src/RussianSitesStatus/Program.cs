using RussianSitesStatus.Services;
using RussianSitesStatus.Models;
using RussianSitesStatus.BackgroundServices;
using RussianSitesStatus.Services.Contracts;

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
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});


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