namespace Umbraco.Cms.Core.Events;

/// <summary>
///     The filter used in the <see cref="IEventDispatcher" /> GetEvents method which determines
///     how the result list is filtered
/// </summary>
public enum EventDefinitionFilter
{
    /// <summary>
    ///     Returns all events tracked
    /// </summary>
    All,

    /// <summary>
    ///     Deduplicates events and only returns the first duplicate instance tracked
    /// </summary>
    FirstIn,

    /// <summary>
    ///     Deduplicates events and only returns the last duplicate instance tracked
    /// </summary>
    LastIn,
}
