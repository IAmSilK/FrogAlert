using FrogAlert.Database;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using FrogAlert.Alerts;
using FrogAlert.Alerts.SMS;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// Add database
builder.Services.AddDbContext<FrogAlertDbContext>(ServiceLifetime.Transient);

// Add alert providers
builder.Services.AddSingleton<SMSAlertProvider>();

// Add environment monitor service
builder.Services.AddHostedService<MonitorService>();

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

// Ensure up-to-date database schema
using (var dbContext = app.Services.GetRequiredService<FrogAlertDbContext>())
{
    dbContext.Database.Migrate();
}

app.Run();
