using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.Tour;
using CharArrays = Umbraco.Core.Constants.CharArrays;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    public class TourController : UmbracoAuthorizedJsonController
    {
        private readonly TourFilterCollection _filters;

        public TourController(TourFilterCollection filters)
        {
            _filters = filters;
        }

        public IEnumerable<BackOfficeTourFile> GetTours()
        {
            var result = new List<BackOfficeTourFile>();

            if (Current.Configs.Settings().BackOffice.Tours.EnableTours == false)
                return result;

            var user = Composing.Current.UmbracoContext.Security.CurrentUser;
            if (user == null)
                return result;

            //get all filters that will be applied to all tour aliases
            var aliasOnlyFilters = _filters.Where(x => x.PluginName == null && x.TourFileName == null).ToList();

            //don't pass in any filters for core tours that have a plugin name assigned
            var nonPluginFilters = _filters.Where(x => x.PluginName == null).ToList();

            //add core tour files
            var coreToursPath = Path.Combine(IOHelper.MapPath(SystemDirectories.Config), "BackOfficeTours");
            if (Directory.Exists(coreToursPath))
            {
                foreach (var tourFile in Directory.EnumerateFiles(coreToursPath, "*.json"))
                {
                    TryParseTourFile(tourFile, result, nonPluginFilters, aliasOnlyFilters);
                }
            }

            //collect all tour files in packages
            var appPlugins = IOHelper.MapPath(SystemDirectories.AppPlugins);
            if (Directory.Exists(appPlugins))
            {
                foreach (var plugin in Directory.EnumerateDirectories(appPlugins))
                {
                    var pluginName = Path.GetFileName(plugin.TrimEnd(CharArrays.Backslash));
                    var pluginFilters = _filters.Where(x => x.PluginName != null && x.PluginName.IsMatch(pluginName))
                        .ToList();

                    //If there is any filter applied to match the plugin only (no file or tour alias) then ignore the plugin entirely
                    var isPluginFiltered = pluginFilters.Any(x => x.TourFileName == null && x.TourAlias == null);
                    if (isPluginFiltered) continue;

                    //combine matched package filters with filters not specific to a package
                    var combinedFilters = nonPluginFilters.Concat(pluginFilters).ToList();

                    foreach (var backofficeDir in Directory.EnumerateDirectories(plugin, "backoffice"))
                    {
                        foreach (var tourDir in Directory.EnumerateDirectories(backofficeDir, "tours"))
                        {
                            foreach (var tourFile in Directory.EnumerateFiles(tourDir, "*.json"))
                            {
                                TryParseTourFile(tourFile, result, combinedFilters, aliasOnlyFilters, pluginName);
                            }
                        }
                    }
                }
            }

            //Get all allowed sections for the current user
            var allowedSections = user.AllowedSections.ToList();

            var toursToBeRemoved = new List<BackOfficeTourFile>();

            //Checking to see if the user has access to the required tour sections, else we remove the tour
            foreach (var backOfficeTourFile in result)
            {
                if (backOfficeTourFile.Tours != null)
                {
                    foreach (var tour in backOfficeTourFile.Tours)
                    {
                        if (tour.RequiredSections != null)
                        {
                            foreach (var toursRequiredSection in tour.RequiredSections)
                            {
                                if (allowedSections.Contains(toursRequiredSection) == false)
                                {
                                    toursToBeRemoved.Add(backOfficeTourFile);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return result.Except(toursToBeRemoved).OrderBy(x => x.FileName, StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Gets a tours for a specific doctype
        /// </summary>
        /// <param name="doctypeAlias">The documenttype alias</param>
        /// <returns>A <see cref="BackOfficeTour"/></returns>
        public IEnumerable<BackOfficeTour> GetToursForDoctype(string doctypeAlias)
        {
            var tourFiles = this.GetTours();

            var doctypeAliasWithCompositions = new List<string>
                                                   {
                                                       doctypeAlias
                                                   };

            var contentType = this.Services.ContentTypeService.Get(doctypeAlias);

            if (contentType != null)
            {
                doctypeAliasWithCompositions.AddRange(contentType.CompositionAliases());
            }

            return tourFiles.SelectMany(x => x.Tours)
                .Where(x =>
                    {
                        if (string.IsNullOrEmpty(x.ContentType))
                        {
                            return false;
                        }
                        var contentTypes = x.ContentType.Split(CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries).Select(ct => ct.Trim());
                        return contentTypes.Intersect(doctypeAliasWithCompositions).Any();
                    });
        }

        private void TryParseTourFile(string tourFile,
            ICollection<BackOfficeTourFile> result,
            List<BackOfficeTourFilter> filters,
            List<BackOfficeTourFilter> aliasOnlyFilters,
            string pluginName = null)
        {
            var fileName = Path.GetFileNameWithoutExtension(tourFile);
            if (fileName == null) return;

            //get the filters specific to this file
            var fileFilters = filters.Where(x => x.TourFileName != null && x.TourFileName.IsMatch(fileName)).ToList();

            //If there is any filter applied to match the file only (no tour alias) then ignore the file entirely
            var isFileFiltered = fileFilters.Any(x => x.TourAlias == null);
            if (isFileFiltered) return;

            //now combine all aliases to filter below
            var aliasFilters = aliasOnlyFilters.Concat(filters.Where(x => x.TourAlias != null))
                .Select(x => x.TourAlias)
                .ToList();

            try
            {
                var contents = File.ReadAllText(tourFile);
                var tours = JsonConvert.DeserializeObject<BackOfficeTour[]>(contents);

                var backOfficeTours = tours.Where(x =>
                    aliasFilters.Count == 0 || aliasFilters.All(filter => filter.IsMatch(x.Alias)) == false);

                var localizedTours = backOfficeTours.Where(x =>
                    string.IsNullOrWhiteSpace(x.Culture) || x.Culture.Equals(Security.CurrentUser.Language,
                        StringComparison.InvariantCultureIgnoreCase)).ToList();

                var tour = new BackOfficeTourFile
                {
                    FileName = Path.GetFileNameWithoutExtension(tourFile),
                    PluginName = pluginName,
                    Tours = localizedTours
                };

                //don't add if all of the tours are filtered
                if (tour.Tours.Any())
                    result.Add(tour);
            }
            catch (IOException e)
            {
                throw new IOException("Error while trying to read file: " + tourFile, e);
            }
            catch (JsonReaderException e)
            {
                throw new JsonReaderException("Error while trying to parse content as tour data: " + tourFile, e);
            }
        }
    }
}
