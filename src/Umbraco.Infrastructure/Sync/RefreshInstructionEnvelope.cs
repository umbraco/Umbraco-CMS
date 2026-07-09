using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.Sync;

/// <summary>
///     Used for any 'Batched' <see cref="IServerMessenger" /> instances which specifies a set of
///     <see cref="RefreshInstruction" /> targeting a collection of
///     <see cref="IServerAddress" />
/// </summary>
public sealed class RefreshInstructionEnvelope
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Sync.RefreshInstructionEnvelope"/> class.
    /// </summary>
    /// <param name="refresher">An <see cref="ICacheRefresher"/> instance representing the cache refresher to use.</param>
    /// <param name="instructions">A collection of <see cref="RefreshInstruction"/> objects containing the refresh instructions to be processed.</param>
    public RefreshInstructionEnvelope(ICacheRefresher refresher, IEnumerable<RefreshInstruction> instructions)
    {
        Refresher = refresher;
        Instructions = instructions;
    }

    /// <summary>
    /// Gets or sets the cache refresher for this refresh instruction envelope.
    /// </summary>
    public ICacheRefresher Refresher { get; set; }

    /// <summary>
    /// Gets or sets the collection of refresh instructions contained in this envelope.
    /// </summary>
    public IEnumerable<RefreshInstruction> Instructions { get; set; }
}
