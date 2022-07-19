using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Tour;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
public class TourController : UmbracoAuthorizedJsonController
{
    private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
    private readonly IContentTypeService _contentTypeService;
    private readonly TourFilterCollection _filters;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly TourSettings _tourSettings;

    public TourController(
        TourFilterCollection filters,
        IHostingEnvironment hostingEnvironment,
        IOptionsSnapshot<TourSettings> tourSettings,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IContentTypeService contentTypeService)
    {
        _filters = filters;
        _hostingEnvironment = hostingEnvironment;

        _tourSettings = tourSettings.Value;
        _backofficeSecurityAccessor = backofficeSecurityAccessor;
        _contentTypeService = contentTypeService;
    }

    public async Task<IEnumerable<BackOfficeTourFile>> GetTours()
    {
        var result = new List<BackOfficeTourFile>();

        if (_tourSettings.EnableTours == false)
        {
            return result;
        }

        IUser? user = _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
        if (user == null)
        {
            return result;
        }

        //get all filters that will be applied to all tour aliases
        var aliasOnlyFilters = _filters.Where(x => x.PluginName == null && x.TourFileName == null).ToList();

        //don't pass in any filters for core tours that have a plugin name assigned
        var nonPluginFilters = _filters.Where(x => x.PluginName == null).ToList();

        //add core tour files
        IEnumerable<string> embeddedTourNames = GetType()
            .Assembly
            .GetManifestResourceNames()
            .Where(x => x.StartsWith("Umbraco.Cms.Web.BackOffice.EmbeddedResources.Tours."));

        foreach (var embeddedTourName in embeddedTourNames)
        {
            await TryParseTourFile(embeddedTourName, result, nonPluginFilters, aliasOnlyFilters, async x => await GetContentFromEmbeddedResource(x));
        }


        //collect all tour files in packages
        var appPlugins = _hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.AppPlugins);
        if (Directory.Exists(appPlugins))
        {
            foreach (var plugin in Directory.EnumerateDirectories(appPlugins))
            {
                var pluginName = Path.GetFileName(plugin.TrimEnd(Constants.CharArrays.Backslash));
                var pluginFilters = _filters.Where(x => x.PluginName != null && x.PluginName.IsMatch(pluginName))
                    .ToList();

                //If there is any filter applied to match the plugin only (no file or tour alias) then ignore the plugin entirely
                var isPluginFiltered = pluginFilters.Any(x => x.TourFileName == null && x.TourAlias == null);
                if (isPluginFiltered)
                {
                    continue;
                }

                //combine matched package filters with filters not specific to a package
                var combinedFilters = nonPluginFilters.Concat(pluginFilters).ToList();

                foreach (var backofficeDir in Directory.EnumerateDirectories(plugin, "backoffice"))
                {
                    foreach (var tourDir in Directory.EnumerateDirectories(backofficeDir, "tours"))
                    {
                        foreach (var tourFile in Directory.EnumerateFiles(tourDir, "*.json"))
                        {
                            await TryParseTourFile(
                                tourFile,
                                result,
                                combinedFilters,
                                aliasOnlyFilters,
                                async x => await System.IO.File.ReadAllTextAsync(x),
                                pluginName);
                        }
                    }
                }
            }
        }

        //Get all allowed sections for the current user
        var allowedSections = user.AllowedSections.ToList();

        var toursToBeRemoved = new List<BackOfficeTourFile>();

        //Checking to see if the user has access to the required tour sections, else we remove the tour
        foreach (BackOfficeTourFile backOfficeTourFile in result)
        {
            if (backOfficeTourFile.Tours != null)
            {
                foreach (BackOfficeTour tour in backOfficeTourFile.Tours)
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

    private async Task<string> GetContentFromEmbeddedResource(string fileName)
    {
        Stream? resourceStream = GetType().Assembly.GetManifestResourceStream(fileName);

        if (resourceStream is null)
        {
            return string.Empty;
        }

        using var reader = new StreamReader(resourceStream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    /// <summary>
    ///     Gets a tours for a specific doctype
    /// </summary>
    /// <param name="doctypeAlias">The documenttype alias</param>
    /// <returns>A <see cref="BackOfficeTour" /></returns>
    public async Task<IEnumerable<BackOfficeTour>> GetToursForDoctype(string doctypeAlias)
    {
        IEnumerable<BackOfficeTourFile> tourFiles = await GetTours();

        var doctypeAliasWithCompositions = new List<string> { doctypeAlias };

        IContentType? contentType = _contentTypeService.Get(doctypeAlias);

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

                IEnumerable<string> contentTypes = x.ContentType
                    .Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries).Select(ct => ct.Trim());
                return contentTypes.Intersect(doctypeAliasWithCompositions).Any();
            });
    }

    private async Task TryParseTourFile(
        string tourFile,
        ICollection<BackOfficeTourFile> result,
        List<BackOfficeTourFilter> filters,
        List<BackOfficeTourFilter> aliasOnlyFilters,
        Func<string, Task<string>> fileNameToFileContent,
        string? pluginName = null)
    {
        var fileName = Path.GetFileNameWithoutExtension(tourFile);
        if (fileName == null)
        {
            return;
        }

        //get the filters specific to this file
        var fileFilters = filters.Where(x => x.TourFileName != null && x.TourFileName.IsMatch(fileName)).ToList();

        //If there is any filter applied to match the file only (no tour alias) then ignore the file entirely
        var isFileFiltered = fileFilters.Any(x => x.TourAlias == null);
        if (isFileFiltered)
        {
            return;
        }

        //now combine all aliases to filter below
        var aliasFilters = aliasOnlyFilters.Concat(filters.Where(x => x.TourAlias != null))
            .Select(x => x.TourAlias)
            .ToList();

        try
        {
            var contents = await fileNameToFileContent(tourFile);
            BackOfficeTour[]? tours = JsonConvert.DeserializeObject<BackOfficeTour[]>(contents);

            IEnumerable<BackOfficeTour>? backOfficeTours = tours?.Where(x =>
                aliasFilters.Count == 0 || aliasFilters.WhereNotNull().All(filter => filter.IsMatch(x.Alias)) == false);

            IUser? user = _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;

            var localizedTours = backOfficeTours?.Where(x =>
                string.IsNullOrWhiteSpace(x.Culture) || x.Culture.Equals(user?.Language, StringComparison.InvariantCultureIgnoreCase)).ToList();

            var tour = new BackOfficeTourFile
            {
                FileName = Path.GetFileNameWithoutExtension(tourFile),
                PluginName = pluginName,
                Tours = localizedTours ?? new List<BackOfficeTour>()
            };

            //don't add if all of the tours are filtered
            if (tour.Tours.Any())
            {
                result.Add(tour);
            }
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
