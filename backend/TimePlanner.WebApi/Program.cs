using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Hosting.WindowsServices;
using TimePlanner.DataAccess;
using TimePlanner.WebApi.Exceptions;
using TimePlanner.WebApi.Mappers;
using TimePlanner.WebApi.Models.Requests;
using TimePlanner.WebApi.Services;
using TimePlanner.WebApi.Validators;
using Microsoft.EntityFrameworkCore;
using TimePlanner.DataAccess.Mappers;
using TimePlanner.DataAccess.Repositories;
using TimePlanner.Domain.Interfaces;
using TimePlanner.Domain.Services;

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
builder.Services.AddScoped<IStatusRepository, StatusRepository>();
builder.Services.AddScoped<IWorkItemRepository, WorkItemRepository>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<IWorkItemService, WorkItemService>();
builder.Services.AddScoped<IStatusResponseMapper, StatusMapper>();
builder.Services.AddScoped<IWorkItemResponseMapper, WorkItemResponseMapper>();
builder.Services.AddScoped<IStatusEntityMapper, StatusEntityMapper>();
builder.Services.AddScoped<IWorkItemEntityMapper, WorkItemEntityMapper>();
builder.Services.AddScoped<IValidator<CreateWorkItemRequest>, WorkItemCreateRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateWorkItemRequest>, WorkItemUpdateRequestValidator>();
builder.Services.AddScoped<ITimeProvider, ProductionTimeProvider>();
builder.Services.AddScoped<IRecurrenceService, RecurrenceService>();
builder.Services.AddDbContext<TimePlannerDbContext>(
  o => o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Host.UseWindowsService();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseCors(x => x
  .AllowAnyOrigin()
  .AllowAnyMethod()
  .AllowAnyHeader());

app.UseAuthorization();
app.UseStaticFiles();

app.MapControllers();

app.UseExceptionHandler(applicationBuilder =>
{
  applicationBuilder.Run(
    async httpContext => { await GlobalExceptionHandler.HandleException(httpContext); });
});

app.Run();
