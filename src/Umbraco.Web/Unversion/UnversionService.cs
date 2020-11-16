using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Web.Unversion
{
    public class UnversionService : IUnversionService
    {
        private readonly ILogger _logger;
        private readonly IUmbracoContextFactory _context;
        private readonly IContentService _contentService;
        private IUnversionConfig _config;

        public UnversionService(IUnversionConfig config, ILogger logger, IUmbracoContextFactory context, IContentService contentService)
        {
            _logger = logger;
            _config = config;
            _context = context;
            _contentService = contentService;
        }

        public void Unversion(IContent content)
        {
            var configEntries = new List<UnversionConfigEntry>();

            if (_config.ConfigEntries.ContainsKey(content.ContentType.Alias))
                configEntries.AddRange(_config.ConfigEntries[content.ContentType.Alias]);

            if (_config.ConfigEntries.ContainsKey(UnversionConfig.AllDocumentTypesKey))
                configEntries.AddRange(_config.ConfigEntries[UnversionConfig.AllDocumentTypesKey]);

            if (configEntries.Count <= 0)
            {
                _logger.Debug<UnversionService>("No unversion configuration found for type {alias}", content.ContentType.Alias);
                return;
            }

            foreach (var configEntry in configEntries)
            {
                var isValid = true;

                // Check the RootXPath if configured
                if (!string.IsNullOrEmpty(configEntry.RootXPath))
                    // TODO: Fix in some otherway
                    if (content.Level > 1 && content.ParentId > 0)
                    {
                        var ids = GetNodeIdsFromXpath(configEntry.RootXPath);
                        isValid = ids.Contains(content.ParentId);
                    }

                if (!isValid)
                {
                    _logger.Debug<UnversionService>("Configuration invalid, rootXPath must be {rootXPath}", configEntry.RootXPath);
                    continue;
                }

                var allVersions = _contentService.GetVersionsSlim(content.Id, 0, int.MaxValue).ToList();

                if (!allVersions.Any())
                {
                    _logger.Debug<UnversionService>("No versions of content {contentId} found", content.Id);
                    continue;
                }

                var versionIdsToDelete = GetVersionsToDelete(allVersions, configEntry, DateTime.Now);

                foreach (var vid in versionIdsToDelete)
                {
                    _logger.Debug<UnversionService>("Deleting version {versionId} of content {contentId}", vid, content.Id);
                    _contentService.DeleteVersion(content.Id, vid, false);
                }

            }

        }

        /// <summary>
        /// Iterates a list of IContent versions and returns items to be removed based on a configEntry.
        /// </summary>
        /// <param name="versions"></param>
        /// <param name="configEntry"></param>
        /// <param name="currentDateTime"></param>
        /// <returns></returns>
        public List<int> GetVersionsToDelete(List<IContent> versions, UnversionConfigEntry configEntry, DateTime currentDateTime)
        {
            var versionIdsToDelete = new List<int>();

            var iterationCount = 0;


            _logger.Debug<UnversionService>("Getting versions for config entry. {alias}, {maxCount}, {maxDays}, {rootXpath}", configEntry.DocTypeAlias, configEntry.MaxCount, configEntry.MaxDays, configEntry.RootXPath);

            foreach (var version in versions)
            {
                iterationCount++;
                _logger.Debug<UnversionService>("Comparing version {versionId}, iterationCount is {iterationCount}", version.VersionId, iterationCount);

                // If we have a maxCount and the current iteration is above that max-count
                if (configEntry.MaxCount > 0 && iterationCount > configEntry.MaxCount)
                {
                    _logger.Debug<UnversionService>("Remove version {versionId}, because iterationCount is {iterationCount} and max count is {maxCount}", version.VersionId, iterationCount, configEntry.MaxCount);
                    versionIdsToDelete.Add(version.VersionId);
                    // no need to compare dates since we've already added this version for deletion
                    continue;
                }

                // If we have a max days and the current version is older
                if (configEntry.MaxDays > 0 && configEntry.MaxDays != int.MaxValue)
                {
                    var dateRemoveBefore = currentDateTime.AddDays(0 - configEntry.MaxDays);
                    if (version.UpdateDate < dateRemoveBefore)
                    {
                        _logger.Debug<UnversionService>("Remove version {versionId}, because version is updated {updateDate} and max days is {maxDays} (cutoff: {dateRemoveBefore})", version.VersionId, version.UpdateDate, configEntry.MaxDays, dateRemoveBefore);
                        versionIdsToDelete.Add(version.VersionId);
                    }
                }

            }

            return versionIdsToDelete;

        }

        private List<int> GetNodeIdsFromXpath(string xpath)
        {
            using (var ctx = _context.EnsureUmbracoContext())
            {
                var nodes = ctx.UmbracoContext.Content.GetByXPath(xpath);

                if (nodes == null)
                    return new List<int>();

                return nodes.Select(x => x.Id).ToList();
            }
        }
    }
}
