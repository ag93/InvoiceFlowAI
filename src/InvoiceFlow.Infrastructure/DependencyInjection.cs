using Azure.AI.ContentUnderstanding;
using Azure.Identity;
using InvoiceFlow.Application.Interfaces;
using InvoiceFlow.Infrastructure.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,  IConfiguration configuration)
    {
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
        return services;
    }
}