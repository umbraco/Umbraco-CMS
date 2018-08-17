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
            // fixme isn't this done in ContentService already?
            var counter = 0;
            var contentForRelease = _contentService.GetContentForRelease().ToArray();
            if (contentForRelease.Length > 0)
                _logger.Debug<ScheduledPublisher>("There's {ContentItemsForRelease} item(s) of content to be published", contentForRelease.Length);
            foreach (var d in contentForRelease)
            {
                try
                {
                    d.ReleaseDate = null;
                    d.PublishCulture(); // fixme variants?
                    var result = _contentService.SaveAndPublish(d, userId: _userService.GetProfileById(d.WriterId).Id);
                    _logger.Debug<ContentService>("Result of publish attempt: {PublishResult}", result.Result);
                    if (result.Success == false)
                    {
                        _logger.Error<ScheduledPublisher>(null, "Error publishing node {NodeId}", d.Id);
                    }
                    else
                    {
                        counter++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error<ScheduledPublisher>(ex, "Error publishing node {NodeId}", d.Id);
                    throw;
                }
            }

            var contentForExpiration = _contentService.GetContentForExpiration().ToArray();
            if (contentForExpiration.Length > 0)
                _logger.Debug<ScheduledPublisher>("There's {ContentItemsForExpiration} item(s) of content to be unpublished", contentForExpiration.Length);
            foreach (var d in contentForExpiration)
            {
                try
                {
                    d.ExpireDate = null;
                    var result = _contentService.Unpublish(d, userId: _userService.GetProfileById(d.WriterId).Id);
                    if (result.Success)
                    {
                        counter++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error<ScheduledPublisher>(ex, "Error unpublishing node {NodeId}", d.Id);
                    throw;
                }
            }

            return counter;
        }
    }
}
