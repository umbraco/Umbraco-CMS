namespace Umbraco.Cms.Infrastructure.Examine;

internal interface IDeferredAction
{
    /// <summary>
    /// Executes the action represented by this deferred action instance.
    /// </summary>
    void Execute();
}
