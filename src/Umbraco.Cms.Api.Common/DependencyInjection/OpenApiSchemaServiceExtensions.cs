using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Api.Common.DependencyInjection;

/// <summary>
/// Extension methods for replacing the internal Microsoft.AspNetCore.OpenApi schema service registration.
/// </summary>
internal static class OpenApiSchemaServiceExtensions
{
    /// <summary>
    /// Replaces the internal Microsoft <c>OpenApiSchemaService</c> registration for the specified document so that schema
    /// generation uses the named <see cref="JsonOptions"/> rather than the default HTTP JSON options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="documentName">The OpenAPI document key (matches the keyed singleton registered by <c>AddOpenApi(documentName)</c>).</param>
    /// <param name="jsonOptionsName">The named <see cref="JsonOptions"/> to use during schema generation for this document.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    /// <remarks>
    /// Workaround for <see href="https://github.com/dotnet/aspnetcore/issues/66340">dotnet/aspnetcore#66340</see>.
    /// </remarks>
    public static IServiceCollection ReplaceOpenApiSchemaService(
        this IServiceCollection services,
        string documentName,
        string jsonOptionsName)
    {
        const string openApiSchemaServiceFullName = "Microsoft.AspNetCore.OpenApi.OpenApiSchemaService";

        ServiceDescriptor descriptor = services.FirstOrDefault(sd =>
            sd.ServiceType.FullName == openApiSchemaServiceFullName
            && Equals(sd.ServiceKey, documentName))
            ?? throw new InvalidOperationException(
                $"Could not find a registration for {openApiSchemaServiceFullName} keyed with '{documentName}'. "
                + $"Ensure AddOpenApi(\"{documentName}\") has been called before {nameof(ReplaceOpenApiSchemaService)}, "
                + "or check whether the internal Microsoft.AspNetCore.OpenApi registration shape has changed.");

        services.Remove(descriptor);
        services.AddKeyedSingleton(
            descriptor.ServiceType,
            documentName,
            (sp, key) => ActivatorUtilities.CreateInstance(
                sp,
                descriptor.ServiceType,
                key,
                Options.Create(sp.GetRequiredService<IOptionsMonitor<JsonOptions>>().Get(jsonOptionsName))));

        return services;
    }
}
