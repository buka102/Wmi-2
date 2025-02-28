
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Wmi.Api.Data;
using Wmi.Api.Middleware;
using Wmi.Api.Models;
using Wmi.Api.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    // Configure Serilog
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();
}

builder.Host.UseSerilog();

// Configure JSON options globally
builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IDataRepository, DataRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IBuyerService, BuyerService>();
builder.Services.AddScoped<INotify, MockNotify>();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});;


// builder.Services.AddScoped<IValidator<Product>, ProductValidator>();
// builder.Services.AddScoped<IValidator<Buyer>, BuyerValidator>();

builder.Services.AddValidatorsFromAssemblyContaining<ProductValidator>();

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();
app.Run();