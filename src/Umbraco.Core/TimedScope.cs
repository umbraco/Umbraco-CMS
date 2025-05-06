using System.Diagnostics;

namespace Umbraco.Cms.Core;

/// <summary>
/// Makes a code block timed (take at least a certain amount of time). This class cannot be inherited.
/// </summary>
public sealed class TimedScope : IDisposable, IAsyncDisposable
{
    private readonly TimeSpan _duration;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Stopwatch _stopwatch;

    /// <summary>
    /// Gets the elapsed time.
    /// </summary>
    /// <value>
    /// The elapsed time.
    /// </value>
    public TimeSpan Elapsed => _stopwatch.Elapsed;

    /// <summary>
    /// Gets the remaining time.
    /// </summary>
    /// <value>
    /// The remaining time.
    /// </value>
    public TimeSpan Remaining
        => TryGetRemaining(out TimeSpan remaining) ? remaining : TimeSpan.Zero;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimedScope" /> class.
    /// </summary>
    /// <param name="millisecondsDuration">The number of milliseconds the scope should at least take.</param>
    public TimedScope(long millisecondsDuration)
        : this(TimeSpan.FromMilliseconds(millisecondsDuration))
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimedScope" /> class.
    /// </summary>
    /// <param name="millisecondsDuration">The number of milliseconds the scope should at least take.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public TimedScope(long millisecondsDuration, CancellationToken cancellationToken)
        : this(TimeSpan.FromMilliseconds(millisecondsDuration), cancellationToken)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimedScope"/> class.
    /// </summary>
    /// <param name="duration">The duration the scope should at least take.</param>
    public TimedScope(TimeSpan duration)
        : this(duration, new CancellationTokenSource())
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimedScope" /> class.
    /// </summary>
    /// <param name="duration">The duration the scope should at least take.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public TimedScope(TimeSpan duration, CancellationToken cancellationToken)
        : this(duration, CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
    { }

    private TimedScope(TimeSpan duration, CancellationTokenSource cancellationTokenSource)
    {
        _duration = duration;
        _cancellationTokenSource = cancellationTokenSource;
        _stopwatch = new Stopwatch();
        _stopwatch.Start();
    }

    /// <summary>
    /// Cancels the timed scope.
    /// </summary>
    public void Cancel()
        => _cancellationTokenSource.Cancel();

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <remarks>
    /// This will block using <see cref="Thread.Sleep(TimeSpan)" /> until the remaining time has elapsed, if not cancelled.
    /// </remarks>
    public void Dispose()
    {
        if (_cancellationTokenSource.IsCancellationRequested is false &&
            TryGetRemaining(out TimeSpan remaining))
        {
            Thread.Sleep(remaining);
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous dispose operation.
    /// </returns>
    /// <remarks>
    /// This will delay using <see cref="Task.Delay(TimeSpan, CancellationToken)" /> until the remaining time has elapsed, if not cancelled.
    /// </remarks>
    public async ValueTask DisposeAsync()
    {
        if (_cancellationTokenSource.IsCancellationRequested is false &&
            TryGetRemaining(out TimeSpan remaining))
        {
            await Task.Delay(remaining, _cancellationTokenSource.Token).ConfigureAwait(false);
        }
    }

    private bool TryGetRemaining(out TimeSpan remaining)
    {
        remaining = _duration.Subtract(Elapsed);

        return remaining > TimeSpan.Zero;
    }
}
