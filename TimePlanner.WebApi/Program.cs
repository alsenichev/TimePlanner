using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Hosting.WindowsServices;
using TimePlanner.DataAccess;
using TimePlanner.Domain.Services.Interfaces;
using TimePlanner.WebApi.Exceptions;
using TimePlanner.WebApi.Mappers;
using TimePlanner.WebApi.Models.Requests;
using TimePlanner.WebApi.Services;
using TimePlanner.WebApi.Validators;

var options = new WebApplicationOptions
{
  Args = args,
  ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default
};

WebApplicationBuilder builder = WebApplication.CreateBuilder(options);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddFluentValidation();
builder.Services.AddSingleton<IStatusRepository, StatusRepository>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<IStatusMapper, StatusMapper>();
builder.Services.AddScoped<IValidator<WorkItemRequest>, WorkItemRequestValidator>();

builder.Host.UseWindowsService();

var app = builder.Build();
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
  applicationBuilder.Run(
    async httpContext => { await GlobalExceptionHandler.HandleException(httpContext); });
});

app.Run();
