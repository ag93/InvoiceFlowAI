using Azure.AI.ContentUnderstanding;
using InvoiceFlow.Application.Interfaces;
using InvoiceFlow.Infrastructure;
using InvoiceFlow.Infrastructure.Storage;
using Azure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
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
