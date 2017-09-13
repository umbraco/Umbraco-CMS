using System;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

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
            var counter = 0;
            var contentForRelease = _contentService.GetContentForRelease().ToArray();
            if (contentForRelease.Length > 0)
                _logger.Debug<ScheduledPublisher>($"There's {contentForRelease.Length} item(s) of content to be published");
            foreach (var d in contentForRelease)
            {
                try
                {
                    d.ReleaseDate = null;
                    var result = _contentService.SaveAndPublishWithStatus(d, d.GetWriterProfile().Id);
                    _logger.Debug<ContentService>($"Result of publish attempt: {result.Result.StatusType}");
                    if (result.Success == false)
                    {
                        if (result.Exception != null)
                        {
                            _logger.Error<ScheduledPublisher>("Could not published the document (" + d.Id + ") based on it's scheduled release, status result: " + result.Result.StatusType, result.Exception);
                        }
                        else
                        {
                            _logger.Warn<ScheduledPublisher>("Could not published the document (" + d.Id + ") based on it's scheduled release. Status result: " + result.Result.StatusType);
                        }
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
                    var result = _contentService.UnPublish(d, d.GetWriterProfile().Id);
                    if (result)
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
