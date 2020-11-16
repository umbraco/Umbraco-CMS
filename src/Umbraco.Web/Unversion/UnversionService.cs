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

        // WB: Note do we want this service in Current.Services etc... ?!
        public UnversionService(IUnversionConfig config, ILogger logger, IUmbracoContextFactory context, IContentService contentService)
        {
            _logger = logger;
            _config = config;
            _context = context;
            _contentService = contentService;
        }

        public void Unversion(IContent content)
        {
            // Get configuration entries for this piece of content
            var configEntries = new List<UnversionConfigEntry>();

            // Add all config entries if it matches our ContentType Alias
            if (_config.ConfigEntries.ContainsKey(content.ContentType.Alias))
                configEntries.AddRange(_config.ConfigEntries[content.ContentType.Alias]);

            // Add all config entries (would it be more than one though?)
            if (_config.ConfigEntries.ContainsKey(UnversionConfig.AllDocumentTypesKey))
                configEntries.AddRange(_config.ConfigEntries[UnversionConfig.AllDocumentTypesKey]);

            if (configEntries.Count <= 0)
            {
                // Would this ever get hit ?
                _logger.Debug<UnversionService>("No unversion configuration found for type {ContentTypeAlias}", content.ContentType.Alias);
                return;
            }

            foreach (var configEntry in configEntries)
            {
                var isValid = true;

                // Check if RootXPath is configured
                if (!string.IsNullOrEmpty(configEntry.RootXPath))
                    // TODO: Fix in some otherway (Check with Soren on what he meant by this)
                    if (content.Level > 1 && content.ParentId > 0)
                    {
                        var ids = GetNodeIdsFromXpath(configEntry.RootXPath);
                        isValid = ids.Contains(content.ParentId);
                    }

                if (!isValid)
                {
                    _logger.Debug<UnversionService>("Invalid RootXPath configuration for '{ContentTypeAlias}' the XPath selector does not contain the parent id '{ContentParentId}' for the current content item '{ContentUdi}'.",
                        configEntry.DocTypeAlias, content.ParentId, content.GetUdi());

                    continue;
                }

                // Get every version of content node
                var allVersions = _contentService.GetVersionsSlim(content.Id, 0, int.MaxValue).ToList();
                if (!allVersions.Any())
                {
                    _logger.Debug<UnversionService>("No versions of content '{ContentUdi}' found", content.GetUdi());
                    continue;
                }

                // Determine versions to delete
                var versionIdsToDelete = GetVersionsToDelete(allVersions, configEntry, DateTime.Now);

                // Delete versions
                foreach (var vid in versionIdsToDelete)
                {
                    _logger.Info<UnversionService>("Deleting version {VersionId} of content '{ContentName}' {ContentUdi}", vid, content.Name, content.GetUdi());

                    // Ensure we only delete the specified version and do NOT delete PRIOR versions
                    _contentService.DeleteVersion(content.Id, vid, false);
                }
            }

        }

        /// <summary>
        /// Iterates a list of IContent versions and returns items to be removed based on a configEntry.
        /// </summary>
        public List<int> GetVersionsToDelete(List<IContent> versions, UnversionConfigEntry configEntry, DateTime currentDateTime)
        {
            var versionIdsToDelete = new List<int>();
            var iterationCount = 0;

            _logger.Debug<UnversionService>("Getting versions for Unversion config entry - ContentType:{ContentTypeAlias} MaxCount:{MaxCount} MaxDays:{MaxDays} MinCount:{MinCount} RootXPath:{RootXpath}", configEntry.DocTypeAlias, configEntry.MaxCount, configEntry.MaxDays, configEntry.MinCount, configEntry.RootXPath);

            foreach (var version in versions)
            {
                iterationCount++;
                _logger.Debug<UnversionService>("Version:{VersionId} ContentUdi:{ContentUdi} IterationCount:{IterationCount}", version.VersionId, version.GetUdi(), iterationCount);

                // If we have a minCount set and current iteration is LESS than min count
                if(configEntry.MinCount > 0 && iterationCount <= configEntry.MinCount)
                {
                    // Do nothing apart from log it as we want to keep this version
                    _logger.Debug<UnversionService>("Keeping version {VersionId} for {ContentUdi} because item was required by MinCount. IterationCount: {IterationCount} MinValue: {MinCount}", version.VersionId, version.GetUdi(), iterationCount, configEntry.MinCount);

                    // no need to compare dates & maxcount since we've already checked this version and we want to keep it
                    continue;
                }

                // If we have a maxCount and the current iteration is MORE that max-count
                if (configEntry.MaxCount > 0 && iterationCount > configEntry.MaxCount)
                {
                    _logger.Debug<UnversionService>("Mark this version to be removed {VersionId} for {ContentUdi} because iterationCount is {IterationCount} and max count is {MaxCount}", version.VersionId, version.GetUdi(), iterationCount, configEntry.MaxCount);
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
                        _logger.Debug<UnversionService>("Mark this version to be removed {VersionId} for {ContentUdi} because version update date {UpdateDate} is less than the cuttoff date {DateRemoveBefore}", version.VersionId, version.GetUdi(), version.UpdateDate, dateRemoveBefore);
                        versionIdsToDelete.Add(version.VersionId);

                        // Added to list stop carrying on
                        continue;
                    }
                }

                // We have a mincount set and the iteration count is higher than the versions we want to keep
                // AND we do NOT have MaxCount or MaxDays sets
                // Thus we need to remove all other versions as we explicity only wanted to keep x min versions only
                if(configEntry.MinCount > 0 && configEntry.MaxCount !> 0 && configEntry.MaxDays !> 0)
                {
                    _logger.Debug<UnversionService>("Mark this version to be removed {VersionId} for {ContentUdi} because we have all the versions we required for mincount and MaxDays nor MaxCount has been set in conjuction.", version.VersionId, version.GetUdi(), iterationCount, configEntry.MaxCount);
                    versionIdsToDelete.Add(version.VersionId);
                }
            }

            return versionIdsToDelete;

        }

        /// <summary>
        /// Gets a list of int NodeIds from an xPath query
        /// </summary>
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
