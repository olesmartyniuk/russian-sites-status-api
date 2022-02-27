using RussianSitesStatus.Services;
using RussianSitesStatus.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<StatusCakeService>();
builder.Services.AddSingleton<Storage<SiteStatus>>();
builder.Services.AddSingleton<Storage<SiteStatusFull>>();
builder.Services.AddHostedService<StatusFetcherBackgroundService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
