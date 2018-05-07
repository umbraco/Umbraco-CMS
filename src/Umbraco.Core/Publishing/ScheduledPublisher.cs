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

        public ScheduledPublisher(IContentService contentService, ILogger logger)
        {
            _contentService = contentService;
            _logger = logger;
        }

        /// <summary>
        /// Processes scheduled operations
        /// </summary>
        /// <returns>
        /// Returns the number of items successfully completed
        /// </returns>
        public int CheckPendingAndProcess()
        {
            // fixme isn't this done in ContentService already?
            var counter = 0;
            var contentForRelease = _contentService.GetContentForRelease().ToArray();
            if (contentForRelease.Length > 0)
                _logger.Debug<ScheduledPublisher>($"There's {contentForRelease.Length} item(s) of content to be published");
            foreach (var d in contentForRelease)
            {
                try
                {
                    d.ReleaseDate = null;
                    d.TryPublishValues(); // fixme variants?
                    var result = _contentService.SaveAndPublish(d, d.GetWriterProfile().Id);
                    _logger.Debug<ContentService>($"Result of publish attempt: {result.Result}");
                    if (result.Success == false)
                    {
                        _logger.Error<ScheduledPublisher>($"Error publishing node {d.Id}");
                    }
                    else
                    {
                        counter++;
                    }
                }
                catch (Exception ee)
                {
                    _logger.Error<ScheduledPublisher>($"Error publishing node {d.Id}", ee);
                    throw;
                }
            }

            var contentForExpiration = _contentService.GetContentForExpiration().ToArray();
            if (contentForExpiration.Length > 0)
                _logger.Debug<ScheduledPublisher>($"There's {contentForExpiration.Length} item(s) of content to be unpublished");
            foreach (var d in contentForExpiration)
            {
                try
                {
                    d.ExpireDate = null;
                    var result = _contentService.Unpublish(d, d.GetWriterProfile().Id);
                    if (result.Success)
                    {
                        counter++;
                    }
                }
                catch (Exception ee)
                {
                    _logger.Error<ScheduledPublisher>($"Error unpublishing node {d.Id}", ee);
                    throw;
                }
            }

            return counter;
        }
    }
}
