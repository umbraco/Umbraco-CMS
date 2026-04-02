namespace Umbraco.Cms.Core.DependencyInjection;

/// <summary>
/// Marker interface indicating the backoffice is enabled.
/// Used to conditionally register Management API controllers and services.
/// </summary>
public interface IBackOfficeEnabledMarker { }

/// <summary>
/// Marker class implementation for <see cref="IBackOfficeEnabledMarker"/>.
/// </summary>
public sealed class BackOfficeEnabledMarker : IBackOfficeEnabledMarker { }
