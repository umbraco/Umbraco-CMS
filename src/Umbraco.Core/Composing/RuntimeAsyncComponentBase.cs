using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Composing;

/// <inheritdoc />
/// <remarks>
/// By default, the component will not execute if Umbraco is restarting or the runtime level is not <see cref="RuntimeLevel.Run" />.
/// </remarks>
public abstract class RuntimeAsyncComponentBase : AsyncComponentBase
{
    private readonly IRuntimeState _runtimeState;

    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeAsyncComponentBase" /> class.
    /// </summary>
    /// <param name="runtimeState">State of the Umbraco runtime.</param>
    protected RuntimeAsyncComponentBase(IRuntimeState runtimeState)
        => _runtimeState = runtimeState;

    /// <inheritdoc />
    protected override bool CanExecute(bool isRestarting)
        => base.CanExecute(isRestarting) && _runtimeState.Level == RuntimeLevel.Run;
}
