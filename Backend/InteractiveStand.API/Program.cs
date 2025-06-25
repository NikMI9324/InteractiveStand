using InteractiveStand.Application.Hubs;
using InteractiveStand.Application.Interfaces;
using InteractiveStand.Application.Services;
using InteractiveStand.Domain.Interfaces;
using InteractiveStand.Infrastructure.Data;
using InteractiveStand.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<RegionDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("Database")));
builder.Services.AddHostedService<MqttService>();

builder.Services.AddScoped<IRegionService, RegionService>();
builder.Services.AddScoped<IPowerDistributionService, PowerDistributionService>();
builder.Services.AddScoped<IRegionRepository,RegionRepository>();
builder.Services.AddScoped<IConnectMessageHandler, ConnectMessageHandler>();

builder.Services.AddSingleton<IPowerDistributionService, PowerDistributionService>();
builder.Services.AddSignalR();

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");


app.MapControllers();
app.MapHub<EnergyDistributionHub>("/energyHub");
app.MapFallbackToFile("index.html");

app.Run();

