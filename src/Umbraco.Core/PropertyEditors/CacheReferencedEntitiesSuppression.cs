namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Provides an ambient, opt-in mechanism to suppress the eager caching of referenced entities performed
/// by <see cref="ICacheReferencedEntities" /> implementations while re-running a value editor's
/// <c>FromEditor</c> pipeline.
/// </summary>
/// <remarks>
/// <para>
/// Upgrade migrations re-run <c>FromEditor</c> purely to re-serialize stored property values. The
/// referenced-entity caching that pipeline performs only benefits the reference tracking of a live save
/// - which migrations do not perform - so during a migration it is wasted work that issues a
/// content/media lookup per processed property. Those lookups run in their own scopes (and, for the
/// parallelized migrations, on separate connections), contending with the migration's own scope.
/// Wrapping the <c>FromEditor</c> call in <see cref="Suppress" /> skips them.
/// </para>
/// <para>
/// This is a companion to the obsolete <see cref="ICacheReferencedEntities" /> and is expected to be
/// removed alongside it once referenced entities are loaded via lazy read locks.
/// </para>
/// </remarks>
[Obsolete("The ICacheReferencedEntities interface is available for support of request caching retrieved entities in property value editors that implement it. " +
          "The intention is to supersede this with lazy loaded read locks, which will make this unnecessary. " +
          "When the interface is removed, these extension methods will also be removed. " +
          "Scheduled for removal in Umbraco 19.")]
public static class CacheReferencedEntitiesSuppression
{
    private static readonly AsyncLocal<bool> _suppressedState = new();

    /// <summary>
    /// Gets a value indicating whether referenced-entity caching is currently suppressed on the executing context.
    /// </summary>
    public static bool IsSuppressed => _suppressedState.Value;

    /// <summary>
    /// Suppresses referenced-entity caching until the returned <see cref="IDisposable" /> is disposed.
    /// </summary>
    /// <returns>A disposable that restores the previous suppression state when disposed.</returns>
    public static IDisposable Suppress() => new SuppressionScope();

    private sealed class SuppressionScope : IDisposable
    {
        private readonly bool _previous;

        public SuppressionScope()
        {
            _previous = _suppressedState.Value;
            _suppressedState.Value = true;
        }

        public void Dispose() => _suppressedState.Value = _previous;
    }
}
