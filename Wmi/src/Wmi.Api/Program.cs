
using System.Text.Json;
using FluentValidation;
using Wmi.Api.Data;
using Wmi.Api.Middleware;
using Wmi.Api.Models;
using Wmi.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IDataRepository, DataRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IBuyerService, BuyerService>();
builder.Services.AddScoped<INotify, MockNotify>();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Use camelCase
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; 

    // Ignore null values globally
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});


// builder.Services.AddScoped<IValidator<Product>, ProductValidator>();
// builder.Services.AddScoped<IValidator<Buyer>, BuyerValidator>();

builder.Services.AddValidatorsFromAssemblyContaining<ProductValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();
app.Run();