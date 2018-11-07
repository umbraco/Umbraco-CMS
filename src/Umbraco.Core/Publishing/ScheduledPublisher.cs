using System;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;

namespace Umbraco.Core.Publishing
{
    /// <summary>
    /// Used to perform scheduled publishing/unpublishing
    /// </summary>
    internal class ScheduledPublisher
    {
        private readonly IContentService _contentService;
        private readonly ILogger _logger;
        private readonly IUserService _userService;

        public ScheduledPublisher(IContentService contentService, ILogger logger, IUserService userService)
        {
            _contentService = contentService;
            _logger = logger;
            _userService = userService;
        }

        /// <summary>
        /// Processes scheduled operations
        /// </summary>
        /// <returns>
        /// Returns the number of items successfully completed
        /// </returns>
        public int CheckPendingAndProcess()
        {
            var results = _contentService.PerformScheduledPublish();
            return results.Count(x => x.Success);
            
        }
    }
}
