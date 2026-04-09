using Examine;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Runtime;

namespace Umbraco.Cms.Infrastructure.Examine;

internal sealed class ExamineIndexingMainDomHandler
{
    private readonly IMainDom _mainDom;
    private readonly IProfilingLogger _profilingLogger;
    private readonly IExamineManager _examineManager;
    private readonly ILogger<ExamineIndexingMainDomHandler> _logger;
    private readonly Lazy<bool> _isMainDom;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Examine.ExamineIndexingMainDomHandler"/> class.
    /// </summary>
    /// <param name="mainDom">The <see cref="IMainDom"/> instance representing the main domain lock.</param>
    /// <param name="profilingLogger">The <see cref="IProfilingLogger"/> used for profiling and logging operations.</param>
    /// <param name="examineManager">The <see cref="IExamineManager"/> responsible for managing Examine indexes.</param>
    /// <param name="logger">The <see cref="ILogger{ExamineIndexingMainDomHandler}"/> instance for logging.</param>
    public ExamineIndexingMainDomHandler(IMainDom mainDom, IProfilingLogger profilingLogger, IExamineManager examineManager, ILogger<ExamineIndexingMainDomHandler> logger)
    {
        _mainDom = mainDom;
        _profilingLogger = profilingLogger;
        _examineManager = examineManager;
        _logger = logger;
        _isMainDom = new Lazy<bool>(DetectMainDom);
    }

    /// <summary>
    /// Determines whether this instance currently holds the MainDom (main domain lock) in the application.
    /// </summary>
    /// <returns><c>true</c> if this instance is the MainDom; otherwise, <c>false</c>.</returns>
    public bool IsMainDom() => _isMainDom.Value;

    private bool DetectMainDom()
    {
        //let's deal with shutting down Examine with MainDom
        var examineShutdownRegistered = _mainDom.Register(release: () =>
        {
            using (_profilingLogger.TraceDuration<ExamineUmbracoIndexingHandler>("Examine shutting down"))
            {
                _examineManager.Dispose();
            }
        });

        if (!examineShutdownRegistered)
        {
            _logger.LogInformation(
                "Examine shutdown not registered, this AppDomain is not the MainDom, Examine will be disabled");

            //if we could not register the shutdown examine ourselves, it means we are not maindom! in this case all of examine should be disabled!
            Suspendable.ExamineEvents.SuspendIndexers(_logger);
            return false; //exit, do not continue
        }

        _logger.LogDebug("Examine shutdown registered with MainDom");
        return true;
    }
}
