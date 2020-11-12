using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Scheduling;

namespace Umbraco.Web.Unversion
{
    public class UnversionComponent : IComponent
    {
        private readonly IRuntimeState _runtime;
        private readonly IProfilingLogger _logger;
        private readonly IContentService _contentService;
        private readonly IUnversionService _unversionService;
        private readonly BackgroundTaskRunner<IBackgroundTask> _unversionRunner;

        public UnversionComponent(IRuntimeState runtime, IProfilingLogger logger, IContentService contentService, IUnversionService unversionService)
        {
            _runtime = runtime;
            _logger = logger;            
            _contentService = contentService;
            _unversionService = unversionService;
            _unversionRunner = new BackgroundTaskRunner<IBackgroundTask>("Unversion", _logger);
        }

        public void Initialize()
        {
            int delayBeforeWeStart = 60000; // 60000ms = 1min
            int howOftenWeRepeat = 6000 * 60; // 60000 * 60 = 1hr

            var task = new UnversionTask(_unversionRunner, delayBeforeWeStart, howOftenWeRepeat, _runtime, _logger, _contentService, _unversionService);

            //As soon as we add our task to the runner it will start to run (after its delay period)
            _unversionRunner.TryAdd(task);
        }

        public void Terminate()
        {
        }
    }
}
