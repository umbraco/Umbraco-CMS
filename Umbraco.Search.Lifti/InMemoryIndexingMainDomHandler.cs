using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Infrastructure;
using Umbraco.Search.Services;

namespace Umbraco.Search.InMemory;

internal class InMemoryIndexingMainDomHandler : ISearchMainDomHandler
{
    private readonly IMainDom _mainDom;
    private readonly IProfilingLogger _profilingLogger;
    private readonly ILogger<InMemoryIndexingMainDomHandler> _logger;
    private readonly Lazy<bool> _isMainDom;

    public InMemoryIndexingMainDomHandler(IMainDom mainDom, IProfilingLogger profilingLogger, ILogger<InMemoryIndexingMainDomHandler> logger)
    {
        _mainDom = mainDom;
        _profilingLogger = profilingLogger;
        _logger = logger;
        _isMainDom = new Lazy<bool>(DetectMainDom);
    }

    public bool IsMainDom() => _isMainDom.Value;

    private bool DetectMainDom()
    {
        //let's deal with shutting down Examine with MainDom
        var examineShutdownRegistered = _mainDom.Register(release: () =>
        {
            using (_profilingLogger.TraceDuration<InMemoryIndexingMainDomHandler>("In memory shutting down"))
            {
            }
        });

        if (!examineShutdownRegistered)
        {
            _logger.LogInformation(
                "In memory shutdown not registered, this AppDomain is not the MainDom, Examine will be disabled");

            //if we could not register the shutdown examine ourselves, it means we are not maindom! in this case all of examine should be disabled!
            Suspendable.ExamineEvents.SuspendIndexers(_logger);
            return false; //exit, do not continue
        }

        _logger.LogDebug("In memory shutdown registered with MainDom");
        return true;
    }
}
