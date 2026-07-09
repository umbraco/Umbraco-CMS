using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Cms.Core.Composing;

/// <summary>
/// Provides lazy resolution of a service from the dependency injection container.
/// </summary>
/// <typeparam name="T">The type of service to resolve.</typeparam>
/// <remarks>
/// This class defers service resolution until the <see cref="Lazy{T}.Value" /> property is accessed,
/// which can help avoid circular dependency issues during application startup.
/// </remarks>
public class LazyResolve<T> : Lazy<T>
    where T : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LazyResolve{T}" /> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve the service.</param>
    public LazyResolve(IServiceProvider serviceProvider)
        : base(serviceProvider.GetRequiredService<T>)
    {
    }
}
