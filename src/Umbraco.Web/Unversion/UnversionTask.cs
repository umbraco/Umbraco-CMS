using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Scheduling;

namespace Umbraco.Web.Unversion
{

    public class UnversionTask : RecurringTaskBase
    {
        private IRuntimeState _runtime;
        private IProfilingLogger _logger;
        private IContentService _contentService;
        private IUnversionService _unversionService;

        public UnversionTask(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayBeforeWeStart, int howOftenWeRepeat, IRuntimeState runtime, IProfilingLogger logger, IContentService contentService, IUnversionService unversionService)
            : base(runner, delayBeforeWeStart, howOftenWeRepeat)
        {
            _runtime = runtime;
            _logger = logger;

            _contentService = contentService;
            _unversionService = unversionService;
        }

        public override bool PerformRun()
        {
            if (_runtime.ServerRole != Core.Sync.ServerRole.Master && _runtime.ServerRole != Core.Sync.ServerRole.Single)
            {
                _logger.Debug<UnversionTask>("Unversion background task does not run when server role '{ServerRole}' is not Master or Single", _runtime.ServerRole);
                return true; // We return true to try again as the server role may change!
            }

            var allContent = new List<IContent>();

            // Add TraceDuration - so users can see if this is taking a long time to calculate
            using (_logger.TraceDuration<UnversionTask>("Calculating number of content nodes in site for Unversion", "Calculated number of nodes in site for Unversion"))
            {               
                // Get all content nodes
                // Worried this could be bad for performance...
                var rootContent = _contentService.GetRootContent();
                allContent.AddRange(rootContent);

                // For each top level root node get its descendants
                foreach (var rootNode in rootContent)
                {
                    var pageIndex = 0;
                    var itemCount = 0;
                    long totalRecords;

                    // Keep paging through all descendants in groups of 100 until we have no more to fetch
                    do
                    {
                        var descendants = _contentService.GetPagedDescendants(rootNode.Id, pageIndex, 100, out totalRecords);

                        pageIndex++;
                        itemCount += descendants.Count();

                        allContent.AddRange(descendants);
                    } while (itemCount < totalRecords);
                }

                _logger.Info<UnversionTask>("The website contains a total number of content nodes {TotalContentNodes} to parse for unversion", allContent.Count);
            }

            // For each piece of content check if we need to unversion
            foreach (var content in allContent)
            {
                _unversionService.Unversion(content);
            }

            // If we want to keep repeating - we need to return true
            // But if we run into a problem/error & want to stop repeating - return false
            return true;
        }

        public override bool IsAsync => false;
    }
}
