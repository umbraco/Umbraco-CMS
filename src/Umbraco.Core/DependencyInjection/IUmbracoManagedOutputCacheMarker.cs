namespace Umbraco.Cms.Core.DependencyInjection;

/// <summary>
/// Marker interface indicating that Umbraco itself has enabled ASP.NET Core output caching
/// (via Website template caching or Delivery API caching configuration).
/// Used to gate Umbraco's automatic registration of the output cache middleware so that
/// applications calling <c>services.AddOutputCache(...)</c> for their own purposes do not
/// inadvertently trigger a duplicate <c>UseOutputCache()</c> registration.
/// </summary>
public interface IUmbracoManagedOutputCacheMarker { }

/// <summary>
/// Marker class implementation for <see cref="IUmbracoManagedOutputCacheMarker"/>.
/// </summary>
public sealed class UmbracoManagedOutputCacheMarker : IUmbracoManagedOutputCacheMarker { }
