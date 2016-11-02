using System;
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

        public ScheduledPublisher(IContentService contentService)
        {
            _contentService = contentService;
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
            foreach (var d in _contentService.GetContentForRelease())
            {
                try
                {
                    d.ReleaseDate = null;
                    var result = _contentService.SaveAndPublishWithStatus(d, (int)d.GetWriterProfile().Id);
                    if (result.Success == false)
                    {
                        if (result.Exception != null)
                        {
                            LogHelper.Error<ScheduledPublisher>("Could not published the document (" + d.Id + ") based on it's scheduled release, status result: " + result.Result.StatusType, result.Exception);
                        }
                        else
                        {
                            LogHelper.Warn<ScheduledPublisher>("Could not published the document (" + d.Id + ") based on it's scheduled release. Status result: " + result.Result.StatusType);
                        }
                    }
                    else
                    {
                        counter++;
                    }
                }
                catch (Exception ee)
                {
                    LogHelper.Error<ScheduledPublisher>(string.Format("Error publishing node {0}", d.Id), ee);
                    throw;
                }
            }
            foreach (var d in _contentService.GetContentForExpiration())
            {
                try
                {
                    d.ExpireDate = null;
                    var result = _contentService.UnPublish(d, (int)d.GetWriterProfile().Id);
                    if (result)
                    {
                        counter++;
                    }
                }
                catch (Exception ee)
                {
                    LogHelper.Error<ScheduledPublisher>(string.Format("Error unpublishing node {0}", d.Id), ee);
                    throw;
                }
            }

            return counter;
        }
    }
}