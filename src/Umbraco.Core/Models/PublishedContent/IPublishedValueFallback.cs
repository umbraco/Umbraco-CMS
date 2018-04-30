namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides a fallback strategy for getting <see cref="IPublishedElement"/> values.
    /// </summary>
    public interface IPublishedValueFallback
    {
        // todo - define & implement

        // property level ... should we move it up to element,
        // so that the decision can be made based upon the entire element, other properties, etc?
        // or, would we need the *two* levels?
    }
}
