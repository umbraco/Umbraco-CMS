using System.Linq.Expressions;

namespace Umbraco.Cms.Api.Management.Routing;

/// <summary>
/// Provides URL generation for Management API controllers from non-controller contexts.
/// </summary>
public interface IManagementApiRouteBuilder
{
    /// <summary>
    /// Generates a path to an action on a Management API controller.
    /// </summary>
    /// <typeparam name="TController">The controller type.</typeparam>
    /// <param name="action">Expression selecting the action name (e.g., c => nameof(c.Schema)).</param>
    /// <param name="routeValues">Route values (e.g., new { id = guid }).</param>
    /// <returns>The generated path, or null if the route could not be found.</returns>
    string? GetPathByAction<TController>(
        Expression<Func<TController, string>> action,
        object? routeValues = null);
}
