namespace Umbraco.Cms.Core.Configuration.UmbracoSettings;

/// <summary>
///     Defines configuration for the type finder used during assembly scanning.
/// </summary>
public interface ITypeFinderConfig
{
    /// <summary>
    ///     Gets the collection of assembly names that should accept load exceptions during type scanning.
    /// </summary>
    IEnumerable<string> AssembliesAcceptingLoadExceptions { get; }
}
