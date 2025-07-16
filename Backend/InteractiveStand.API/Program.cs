using InteractiveStand.Application.Hubs;
using InteractiveStand.Application.Interfaces;
using InteractiveStand.Application.Mapping;
using InteractiveStand.Application.Services;
using InteractiveStand.Application.Services.Mqtt;
using InteractiveStand.Domain.Interfaces;
using InteractiveStand.Infrastructure.Data;
using InteractiveStand.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var containerConnectionString = builder.Configuration.GetConnectionString("Database");
var localConnectionString = "Host=localhost;Port=5432;Database=EnergySystem;Username=postgres;Password=miroshka";

string finalConnectionString;

try
{
    using var conn = new NpgsqlConnection(containerConnectionString);
    conn.Open();
    finalConnectionString = containerConnectionString;
}
catch (Exception)
{
    finalConnectionString = localConnectionString;
}

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<RegionDbContext>(o => o.UseNpgsql(finalConnectionString));
builder.Services.AddSingleton<IMqttSimulationPublisher, MqttSimulationPublisherService>();
builder.Services.AddSingleton<IMqttBackgroundPublisher, MqttMessagesBackgroundService>();
builder.Services.AddHostedService<MqttMessagesBackgroundService>();

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddScoped<IRegionService, RegionService>();
builder.Services.AddScoped<IPowerDistributionService, PowerDistributionService>();
builder.Services.AddScoped<IRegionRepository,RegionRepository>();
builder.Services.AddScoped<IConnectMessageHandler, ConnectMessageHandler>();
builder.Services.AddScoped<IUpdateMessageHandler, UpdateMessagehandler>();

builder.Services.AddSingleton<IPowerDistributionService, PowerDistributionService>();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
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

