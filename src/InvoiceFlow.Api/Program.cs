using Azure;
using Azure.AI.ContentUnderstanding;
using InvoiceFlow.Application.Interfaces;
using InvoiceFlow.Application.Services;
using InvoiceFlow.Application.Validation;
using InvoiceFlow.Infrastructure;
using InvoiceFlow.Infrastructure.Storage;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var databasePath = Path.Combine(builder.Environment.ContentRootPath, "invoiceflow.db");
builder.Configuration["ConnectionStrings:InvoiceDatabase"] = $"Data Source={databasePath}";
builder.Services.AddScoped<InvoiceValidationService>();
builder.Services.AddScoped<InvoiceProcessingService>();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var uploadDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "uploads");
builder.Services.AddSingleton<IDocumentStorage>(_ => new LocalDocumentStorage(uploadDirectory));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
app.Run();
