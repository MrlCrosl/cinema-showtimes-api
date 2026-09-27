using System.Text.Json.Serialization;
using Cinema.Api.ErrorHandling;
using Cinema.Application;
using Cinema.Infrastructure;
using Cinema.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Cinema")
    ?? throw new InvalidOperationException("Connection string 'Cinema' is not configured.");

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddApplication();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services
    // Let FluentValidation own request validation; model state still rejects malformed JSON.
    .AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();

await app.RunAsync();
