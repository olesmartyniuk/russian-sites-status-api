using RussianSitesStatus.Services;
using RussianSitesStatus.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<StatusCakeService>();
builder.Services.AddSingleton<Storage<Site>>();
builder.Services.AddSingleton<Storage<SiteDetails>>();
builder.Services.AddHostedService<StatusFetcherBackgroundService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
