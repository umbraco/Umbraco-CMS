using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Core.Logging;

/// <summary>
///     Starts the timer and invokes a  callback upon disposal. Provides a simple way of timing an operation by wrapping it
///     in a <code>using</code> (C#) statement.
/// </summary>
public class DisposableTimer : DisposableObjectSlim
{
    private readonly string _endMessage;
    private readonly object[]? _endMessageArgs;
    private readonly object[]? _failMessageArgs;
    private readonly LogLevel _level;
    private readonly ILogger _logger;
    private readonly Type _loggerType;
    private readonly IDisposable? _profilerStep;
    private readonly int _thresholdMilliseconds;
    private readonly string _timingId;
    private bool _failed;
    private Exception? _failException;
    private string? _failMessage;

    // internal - created by profiling logger
    internal DisposableTimer(
        ILogger logger,
        LogLevel level,
        IProfiler profiler,
        Type loggerType,
        string startMessage,
        string endMessage,
        string? failMessage = null,
        object[]? startMessageArgs = null,
        object[]? endMessageArgs = null,
        object[]? failMessageArgs = null,
        int thresholdMilliseconds = 0)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _level = level;
        _loggerType = loggerType ?? throw new ArgumentNullException(nameof(loggerType));
        _endMessage = endMessage;
        _failMessage = failMessage;
        _endMessageArgs = endMessageArgs;
        _failMessageArgs = failMessageArgs;
        _thresholdMilliseconds = thresholdMilliseconds < 0 ? 0 : thresholdMilliseconds;
        _timingId = Guid.NewGuid().ToString("N").Substring(0, 7); // keep it short-ish

        if (thresholdMilliseconds == 0)
        {
            switch (_level)
            {
                case LogLevel.Debug:
                    if (startMessageArgs == null)
                    {
                        logger.LogDebug("{StartMessage} [Timing {TimingId}]", startMessage, _timingId);
                    }
                    else
                    {
                        var args = new object[startMessageArgs.Length + 1];
                        startMessageArgs.CopyTo(args, 0);
                        args[startMessageArgs.Length] = _timingId;
                        logger.LogDebug(startMessage + " [Timing {TimingId}]", args);
                    }

                    break;
                case LogLevel.Information:
                    if (startMessageArgs == null)
                    {
                        logger.LogInformation("{StartMessage} [Timing {TimingId}]", startMessage, _timingId);
                    }
                    else
                    {
                        var args = new object[startMessageArgs.Length + 1];
                        startMessageArgs.CopyTo(args, 0);
                        args[startMessageArgs.Length] = _timingId;
                        logger.LogInformation(startMessage + " [Timing {TimingId}]", args);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level));
            }
        }

        // else aren't logging the start message, this is output to the profiler but not the log,
        // we just want the log to contain the result if it's more than the minimum ms threshold.
        _profilerStep = profiler?.Step(loggerType, startMessage);
    }

    public Stopwatch Stopwatch { get; } = Stopwatch.StartNew();

    /// <summary>
    ///     Reports a failure.
    /// </summary>
    /// <param name="failMessage">The fail message.</param>
    /// <param name="exception">The exception.</param>
    /// <remarks>Completion of the timer will be reported as an error, with the specified message and exception.</remarks>
    public void Fail(string? failMessage = null, Exception? exception = null)
    {
        _failed = true;
        _failMessage = failMessage ?? _failMessage ?? "Failed.";
        _failException = exception;
    }

    /// <summary>
    ///     Disposes resources.
    /// </summary>
    /// <remarks>Overrides abstract class <see cref="DisposableObject" /> which handles required locking.</remarks>
    protected override void DisposeResources()
    {
        Stopwatch.Stop();

        _profilerStep?.Dispose();

        if ((Stopwatch.ElapsedMilliseconds >= _thresholdMilliseconds || _failed)
            && _loggerType != null && _logger != null
            && (string.IsNullOrWhiteSpace(_endMessage) == false || _failed))
        {
            if (_failed)
            {
                if (_failMessageArgs is null)
                {
                    _logger.LogError(_failException, "{FailMessage} ({Duration}ms) [Timing {TimingId}]", _failMessage, Stopwatch.ElapsedMilliseconds, _timingId);
                }
                else
                {
                    var args = new object[_failMessageArgs.Length + 2];
                    _failMessageArgs.CopyTo(args, 0);
                    args[_failMessageArgs.Length - 1] = Stopwatch.ElapsedMilliseconds;
                    args[_failMessageArgs.Length] = _timingId;
                    _logger.LogError(_failException, _failMessage + " ({Duration}ms) [Timing {TimingId}]", args);
                }
            }
            else
            {
                switch (_level)
                {
                    case LogLevel.Debug:
                        if (_endMessageArgs == null)
                        {
                            _logger.LogDebug(
                                "{EndMessage} ({Duration}ms) [Timing {TimingId}]",
                                _endMessage,
                                Stopwatch.ElapsedMilliseconds,
                                _timingId);
                        }
                        else
                        {
                            var args = new object[_endMessageArgs.Length + 2];
                            _endMessageArgs.CopyTo(args, 0);
                            args[^1] = Stopwatch.ElapsedMilliseconds;
                            args[args.Length] = _timingId;
                            _logger.LogDebug(_endMessage + " ({Duration}ms) [Timing {TimingId}]", args);
                        }

                        break;
                    case LogLevel.Information:
                        if (_endMessageArgs == null)
                        {
                            _logger.LogInformation(
                                "{EndMessage} ({Duration}ms) [Timing {TimingId}]",
                                _endMessage,
                                Stopwatch.ElapsedMilliseconds,
                                _timingId);
                        }
                        else
                        {
                            var args = new object[_endMessageArgs.Length + 2];
                            _endMessageArgs.CopyTo(args, 0);
                            args[_endMessageArgs.Length - 1] = Stopwatch.ElapsedMilliseconds;
                            args[_endMessageArgs.Length] = _timingId;
                            _logger.LogInformation(_endMessage + " ({Duration}ms) [Timing {TimingId}]", args);
                        }

                        break;

                        // filtered in the ctor
                        // default:
                        //    throw new Exception();
                }
            }
        }
    }
}
