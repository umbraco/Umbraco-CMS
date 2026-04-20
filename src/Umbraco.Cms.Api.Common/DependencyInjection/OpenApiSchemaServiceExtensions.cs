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
    /// Microsoft.AspNetCore.OpenApi resolves a single, default-named <see cref="JsonOptions"/> for schema generation,
    /// which means schema output is shared across every consumer of the host. As a CMS we need different JSON options per
    /// API (e.g. camelCase, custom type resolvers) without polluting the default options that consumer code depends on.
    /// Until <see href="https://github.com/dotnet/aspnetcore/issues/60738"/> is addressed upstream this requires reaching
    /// into the internal <c>OpenApiSchemaService</c> registration. Note that <c>dotnet/aspnet-api-versioning</c>
    /// (Asp.Versioning.OpenApi) uses the same workaround for per-version schema services.
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
