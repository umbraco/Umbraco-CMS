namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Provides a method to try to find and assign an Umbraco document to a <c>PublishedRequest</c>
///     when everything else has failed.
/// </summary>
/// <remarks>Identical to <see cref="IContentFinder" /> but required in order to differentiate them in ioc.</remarks>
public interface IContentLastChanceFinder : IContentFinder
{
}
