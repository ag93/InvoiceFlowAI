using Azure.AI.ContentUnderstanding;
using Azure.Identity;
using InvoiceFlow.Application.Interfaces;
using InvoiceFlow.Infrastructure.AI;
using InvoiceFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,  IConfiguration configuration)
    {
        //AzureContentUnderstanding
        var endpoint = configuration["AzureContentUnderstanding:Endpoint"];

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new InvalidOperationException("Azure Content Understanding endpoint is missing.");
        }

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri) || endpointUri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException("Azure Content Understanding endpoint must be a valid HTTPS URL.");
        }

        services.AddSingleton(new ContentUnderstandingClient(endpointUri, new DefaultAzureCredential()));
        services.AddScoped<IInvoiceExtractor, AzureInvoiceExtractor>();

        //SQLite
        var connectionString = configuration.GetConnectionString("InvoiceDatabase");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Invoice database connection string is missing.");
        }

        services.AddDbContext<InvoiceDbContext>(options =>options.UseSqlite(connectionString));
        return services;
    }
}