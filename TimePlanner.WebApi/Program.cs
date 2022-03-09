using System.Net;
using System.Text.Json;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TimePlanner.DataAccess;
using TimePlanner.Domain.Services.Interfaces;
using TimePlanner.WebApi.Mappers;
using TimePlanner.WebApi.Models.Requests;
using TimePlanner.WebApi.Services;
using TimePlanner.WebApi.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddFluentValidation();
builder.Services.AddSingleton<IStatusRepository, StatusRepository>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<IStatusMapper, StatusMapper>();
builder.Services.AddScoped<IValidator<WorkItemRequest>, WorkItemRequestValidator>();

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

app.UseExceptionHandler(applicationBuilder =>
{
  applicationBuilder.Run(async httpContext =>
  {
    httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
    httpContext.Response.ContentType = "application/json";

    var contextFeature = httpContext.Features.Get<IExceptionHandlerPathFeature>();
    if (contextFeature != null)
    {
      var ex = contextFeature.Error;
      await httpContext.Response.WriteAsync(JsonSerializer.Serialize(
        new ProblemDetails
        {
          Type = ex.GetType().Name,
          Status = (int)HttpStatusCode.InternalServerError,
          Instance = contextFeature?.Path,
          Title = ex.Message,
          Detail = app.Environment.IsDevelopment() ? ex.StackTrace : null
        }));
    }
  });
});

app.Run();
