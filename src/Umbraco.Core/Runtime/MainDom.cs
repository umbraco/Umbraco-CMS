using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Runtime
{

    /// <summary>
    /// Provides the full implementation of <see cref="IMainDom"/>.
    /// </summary>
    /// <remarks>
    /// <para>When an AppDomain starts, it tries to acquire the main domain status.</para>
    /// <para>When an AppDomain stops (eg the application is restarting) it should release the main domain status.</para>
    /// </remarks>
    public class MainDom : IMainDom, IRegisteredObject, IDisposable
    {
        #region Vars

        private readonly ILogger<MainDom> _logger;
        private IApplicationShutdownRegistry? _hostingEnvironment;
        private readonly IMainDomLock _mainDomLock;

        // our own lock for local consistency
        private object _locko = new();

        private bool _isInitialized;
        // indicates whether...
        private bool? _isMainDom; // we are the main domain
        private volatile bool _signaled; // we have been signaled

        // actions to run before releasing the main domain
        private readonly List<KeyValuePair<int, Action>> _callbacks = new();

        private const int LockTimeoutMilliseconds = 40000; // 40 seconds

        private Task? _listenTask;
        private Task? _listenCompleteTask;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MainDom"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="systemLock">The distributed lock implementation.</param>
        public MainDom(ILogger<MainDom> logger, IMainDomLock systemLock)
        {
            _logger = logger;
            _mainDomLock = systemLock;
        }

        #endregion

        /// <inheritdoc/>
        public bool Acquire(IApplicationShutdownRegistry hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));

            return LazyInitializer.EnsureInitialized(ref _isMainDom, ref _isInitialized, ref _locko, () =>
            {
                hostingEnvironment.RegisterObject(this);
                return Acquire();
            })!.Value;
        }

        /// <inheritdoc />
        public bool Register(Action? install = null, Action? release = null, int weight = 100)
        {
            lock (_locko)
            {
                if (_signaled)
                {
                    return false;
                }

                if (_isMainDom.HasValue == false)
                {
                    throw new InvalidOperationException("Register called before IsMainDom has been established");
                }
                else if (_isMainDom == false)
                {
                    _logger.LogWarning("Register called when MainDom has not been acquired");
                    return false;
                }

                install?.Invoke();
                if (release != null)
                {
                    _callbacks.Add(new KeyValuePair<int, Action>(weight, release));
                }

                return true;
            }
        }

        // handles the signal requesting that the main domain is released
        private void OnSignal(string source)
        {
            // once signaled, we stop waiting, but then there is the hosting environment
            // so we have to make sure that we only enter that method once

            lock (_locko)
            {
                if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                {
                    _logger.LogDebug("Signaled ({Signaled}) ({SignalSource})", _signaled ? "again" : "first", source);
                }
                if (_signaled)
                {
                    return;
                }

                if (_isMainDom == false)
                {
                    return; // probably not needed
                }

                _signaled = true;

                try
                {
                    _logger.LogInformation("Stopping ({SignalSource})", source);
                    foreach (Action callback in _callbacks.OrderBy(x => x.Key).Select(x => x.Value))
                    {
                        try
                        {
                            callback(); // no timeout on callbacks
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Error while running callback");
                            continue;
                        }
                    }
                    if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                    {
                        _logger.LogDebug("Stopped ({SignalSource})", source);
                    }
                }
                finally
                {
                    // in any case...
                    _isMainDom = false;
                    _mainDomLock.Dispose();
                    _logger.LogInformation("Released ({SignalSource})", source);
                }

            }
        }

        // acquires the main domain
        private bool Acquire()
        {
            // if signaled, too late to acquire, give up
            // the handler is not installed so that would be the hosting environment
            if (_signaled)
            {
                _logger.LogInformation("Cannot acquire MainDom (signaled).");
                return false;
            }

            _logger.LogInformation("Acquiring MainDom.");

            // Get the lock
            var acquired = false;
            try
            {
                acquired = _mainDomLock.AcquireLockAsync(LockTimeoutMilliseconds).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while acquiring MainDom");
            }

            if (!acquired)
            {
                _logger.LogInformation("Cannot acquire MainDom (timeout).");

                // In previous versions we'd let a TimeoutException be thrown
                // and the appdomain would not start. We have the opportunity to allow it to
                // start without having MainDom? This would mean that it couldn't write
                // to nucache/examine and would only be ok if this was a super short lived appdomain.
                // maybe safer to just keep throwing in this case.

                throw new TimeoutException("Cannot acquire MainDom");
                // return false;
            }

            try
            {
                // Listen for the signal from another AppDomain coming online to release the lock
                _listenTask = _mainDomLock.ListenAsync();
                _listenCompleteTask = _listenTask.ContinueWith(
                    t =>
                {
                    if (_listenTask.Exception != null)
                    {
                        _logger.LogWarning("Listening task completed with {TaskStatus}, Exception: {Exception}", _listenTask.Status, _listenTask.Exception);
                    }
                    else
                    {
                        if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                        {
                            _logger.LogDebug("Listening task completed with {TaskStatus}", _listenTask.Status);
                        }
                    }

                    OnSignal("signal");
                },
                    TaskScheduler.Default); // Must explicitly specify this, see https://blog.stephencleary.com/2013/10/continuewith-is-dangerous-too.html
            }
            catch (OperationCanceledException ex)
            {
                // the waiting task could be canceled if this appdomain is naturally shutting down, we'll just swallow this exception
                _logger.LogWarning(ex, ex.Message);
            }

            _logger.LogInformation("Acquired MainDom.");
            return true;
        }

        /// <inheritdoc />
        public bool IsMainDom
        {
            get
            {
                if (!_isMainDom.HasValue)
                {
                    throw new InvalidOperationException("IsMainDom has not been established yet");
                }
                return _isMainDom.Value;
            }
        }

        /// <inheritdoc />
        void IRegisteredObject.Stop(bool immediate)
        {
            OnSignal("environment"); // will run once

            // The web app is stopping, need to wind down
            Dispose(true);

            _hostingEnvironment?.UnregisterObject(this);
        }

        #region IDisposable Support

        private bool _disposedValue; // To detect redundant calls

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="MainDom"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _mainDomLock.Dispose();
                }

                _disposedValue = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        /// <summary>
        /// Gets a unique identifier for the main domain based on the hosting environment.
        /// </summary>
        /// <param name="hostingEnvironment">The hosting environment.</param>
        /// <returns>A SHA1 hash that uniquely identifies this application instance.</returns>
        /// <remarks>
        /// The ID is generated from the application ID and physical path to ensure uniqueness
        /// across multiple sites running on the same server.
        /// </remarks>
        public static string GetMainDomId(IHostingEnvironment hostingEnvironment)
        {
            // HostingEnvironment.ApplicationID is null in unit tests, making ReplaceNonAlphanumericChars fail
            var appId = hostingEnvironment.ApplicationId?.ReplaceNonAlphanumericChars(string.Empty) ?? string.Empty;

            // combining with the physical path because if running on eg IIS Express,
            // two sites could have the same appId even though they are different.
            //
            // now what could still collide is... two sites, running in two different processes
            // and having the same appId, and running on the same app physical path
            //
            // we *cannot* use the process ID here because when an AppPool restarts it is
            // a new process for the same application path

            var appPath = hostingEnvironment.ApplicationPhysicalPath?.ToLowerInvariant() ?? string.Empty;
            var hash = (appId + ":::" + appPath).GenerateHash<SHA1>();

            return hash;
        }
    }
}
