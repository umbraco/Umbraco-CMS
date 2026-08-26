using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Api.Management.SchemaLockdown;

/// <summary>
/// Infers the <see cref="SchemaOperation"/> implied by an HTTP verb.
/// </summary>
internal static class SchemaOperationResolver
{
    /// <summary>
    /// Resolves the operation inferred from an HTTP verb.
    /// </summary>
    /// <param name="httpMethod">The HTTP method the action responds to.</param>
    /// <returns>The resolved operation.</returns>
    /// <remarks>
    /// <see cref="SchemaOperation.Read"/> is only ever produced by an explicit allowlist of safe verbs. Anything
    /// unrecognised, or absent, resolves to <see cref="SchemaOperation.Unknown"/>, which is denied on every entity
    /// type the restrictions speak to, so a new or custom verb cannot bypass lockdown.
    /// </remarks>
    public static SchemaOperation Resolve(string? httpMethod)
        => httpMethod is null ? SchemaOperation.Unknown : FromVerb(httpMethod);

    private static SchemaOperation FromVerb(string httpMethod)
    {
        if (HttpMethods.IsGet(httpMethod) || HttpMethods.IsHead(httpMethod) || HttpMethods.IsOptions(httpMethod))
        {
            return SchemaOperation.Read;
        }

        if (HttpMethods.IsPost(httpMethod))
        {
            return SchemaOperation.Create;
        }

        if (HttpMethods.IsPut(httpMethod) || HttpMethods.IsPatch(httpMethod))
        {
            return SchemaOperation.Update;
        }

        if (HttpMethods.IsDelete(httpMethod))
        {
            return SchemaOperation.Delete;
        }

        return SchemaOperation.Unknown;
    }
}
