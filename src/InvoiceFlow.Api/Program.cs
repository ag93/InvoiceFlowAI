using InvoiceFlow.Application.Interfaces;
using InvoiceFlow.Application.Services;
using InvoiceFlow.Application.Validation;
using InvoiceFlow.Infrastructure;
using InvoiceFlow.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthorization();
builder.Services.AddControllers();
var databasePath = Path.Combine(builder.Environment.ContentRootPath, "invoiceflow.db");
builder.Configuration["ConnectionStrings:InvoiceDatabase"] = $"Data Source={databasePath}";
builder.Services.AddScoped<InvoiceValidationService>();
builder.Services.AddScoped<InvoiceProcessingService>();
builder.Services.AddScoped<InvoiceApprovalService>();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

var tenantId = builder.Configuration["AzureAd:TenantId"] ?? throw new InvalidOperationException("AzureAd:TenantId is missing.");
var apiClientId = builder.Configuration["AzureAd:ClientId"] ?? throw new InvalidOperationException("AzureAd:ClientId is missing.");
var apiScope = $"api://{apiClientId}/Invoices.Access";

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Description = "Microsoft Entra ID authentication",
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize"),

                    TokenUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token"),

                    Scopes = new Dictionary<string, string>
                    {
                        [apiScope] = "Access InvoiceFlow AI"
                    }
                }
            }
        });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("oauth2", document)] = new List<string> { apiScope }
        });
});

var uploadDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "uploads");
builder.Services.AddSingleton<IDocumentStorage>(_ => new LocalDocumentStorage(uploadDirectory));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.OAuthClientId(builder.Configuration["SwaggerOAuth:ClientId"]);
        options.OAuthUsePkce();
        options.OAuthScopes(apiScope);
        options.OAuthAppName("InvoiceFlow AI Swagger");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
