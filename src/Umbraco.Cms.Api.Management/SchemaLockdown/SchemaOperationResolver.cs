using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Api.Management.SchemaLockdown;

/// <summary>
/// Resolves the <see cref="SchemaOperation"/> an action performs.
/// </summary>
internal static class SchemaOperationResolver
{
    /// <summary>
    /// Resolves the operation, preferring an explicit declaration over inference from the HTTP verb.
    /// </summary>
    /// <param name="httpMethod">The HTTP method the action responds to.</param>
    /// <param name="declared">The declared operation, if any.</param>
    /// <returns>The resolved operation.</returns>
    /// <remarks>
    /// <see cref="SchemaOperation.Read"/> is only ever produced by an explicit allowlist of safe verbs. Anything
    /// unrecognised resolves to <see cref="SchemaOperation.Unknown"/>, which the default policy blocks, so a new
    /// or custom verb cannot bypass lockdown.
    /// </remarks>
    public static SchemaOperation Resolve(string? httpMethod, SchemaOperationAttribute? declared)
    {
        if (declared is not null)
        {
            return declared.Operation;
        }

        return httpMethod is null ? SchemaOperation.Unknown : FromVerb(httpMethod);
    }

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
