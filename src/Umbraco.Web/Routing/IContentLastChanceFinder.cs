namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides a method to try to find and assign an Umbraco document to a <c>PublishedContentRequest</c>
    /// when everything else has failed.
    /// </summary>
    /// <remarks>Identical to <see cref="IContentFinder"/> but required in order to differenciate them in ioc.</remarks>
    public interface IContentLastChanceFinder : IContentFinder
    { }
}
