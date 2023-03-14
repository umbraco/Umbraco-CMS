using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Tour;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
public class TourController : UmbracoAuthorizedJsonController
{
    private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
    private readonly IContentTypeService _contentTypeService;
    private readonly TourFilterCollection _filters;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly TourSettings _tourSettings;

    // IHostingEnvironment is still injected as when removing it, the number of
    // parameters matches with the obsolete ctor and the two ctors become ambiguous
    // [ActivatorUtilitiesConstructor] won't solve the problem in this case
    // IHostingEnvironment can be removed when the obsolete ctor is removed
    [ActivatorUtilitiesConstructor]
    public TourController(
        TourFilterCollection filters,
        IHostingEnvironment hostingEnvironment,
        IOptionsSnapshot<TourSettings> tourSettings,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IContentTypeService contentTypeService,
        IWebHostEnvironment webHostEnvironment)
    {
        _filters = filters;
        _tourSettings = tourSettings.Value;
        _backofficeSecurityAccessor = backofficeSecurityAccessor;
        _contentTypeService = contentTypeService;
        _webHostEnvironment = webHostEnvironment;
    }

    [Obsolete("Use other ctor - Will be removed in Umbraco 13")]
    public TourController(
        TourFilterCollection filters,
        IHostingEnvironment hostingEnvironment,
        IOptionsSnapshot<TourSettings> tourSettings,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IContentTypeService contentTypeService)
        : this(
              filters,
              hostingEnvironment,
              tourSettings,
              backofficeSecurityAccessor,
              contentTypeService,
              StaticServiceProvider.Instance.GetRequiredService<IWebHostEnvironment>())
    {
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

        // Get all filters that will be applied to all tour aliases
        var aliasOnlyFilters = _filters.Where(x => x.PluginName == null && x.TourFileName == null).ToList();

        // Don't pass in any filters for core tours that have a plugin name assigned
        var nonPluginFilters = _filters.Where(x => x.PluginName == null).ToList();


        // Get core tour files
        IFileProvider toursProvider = new EmbeddedFileProvider(GetType().Assembly, "Umbraco.Cms.Web.BackOffice.EmbeddedResources.Tours");

        IEnumerable<IFileInfo> embeddedTourFiles = toursProvider.GetDirectoryContents(string.Empty)
                                    .Where(x => !x.IsDirectory && x.Name.EndsWith(".json"));

        foreach (var embeddedTour in embeddedTourFiles)
        {
            using Stream stream = embeddedTour.CreateReadStream();
            await TryParseTourFile(embeddedTour.Name, result, nonPluginFilters, aliasOnlyFilters, stream);
        }

        // Collect all tour files from packages - /App_Plugins physical or virtual locations
        IEnumerable<Tuple<IFileInfo, string>> toursFromPackages = GetTourFiles(_webHostEnvironment.WebRootFileProvider, Constants.SystemDirectories.AppPlugins);

        foreach (var tuple in toursFromPackages)
        {
            string pluginName = tuple.Item2;
            var pluginFilters = _filters.Where(x => x.PluginName != null && x.PluginName.IsMatch(pluginName)).ToList();

            // Combine matched package filters with filters not specific to a package
            var combinedFilters = nonPluginFilters.Concat(pluginFilters).ToList();

            IFileInfo tourFile = tuple.Item1;
            using (Stream stream = tourFile.CreateReadStream())
            {
                await TryParseTourFile(
                    tourFile.Name,
                    result,
                    combinedFilters,
                    aliasOnlyFilters,
                    stream,
                    pluginName);
            }
        }

        // Get all allowed sections for the current user
        var allowedSections = user.AllowedSections.ToList();

        var toursToBeRemoved = new List<BackOfficeTourFile>();

        // Checking to see if the user has access to the required tour sections, else we remove the tour
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

    private IEnumerable<Tuple<IFileInfo, string>> GetTourFiles(IFileProvider fileProvider, string folder)
    {
        IEnumerable<IFileInfo> pluginFolders = fileProvider.GetDirectoryContents(folder).Where(x => x.IsDirectory);

        foreach (IFileInfo pluginFolder in pluginFolders)
        {
            var pluginFilters = _filters.Where(x => x.PluginName != null && x.PluginName.IsMatch(pluginFolder.Name)).ToList();

            // If there is any filter applied to match the plugin only (no file or tour alias) then ignore the plugin entirely
            var isPluginFiltered = pluginFilters.Any(x => x.TourFileName == null && x.TourAlias == null);
            if (isPluginFiltered)
            {
                continue;
            }

            // get the full virtual path for the plugin folder
            var pluginFolderPath = WebPath.Combine(folder, pluginFolder.Name);

            // loop through the folder(s) in order to find tours
            //  - there could be multiple on case sensitive file system
            // Hard-coding the "backoffice" directory name to gain a better performance when traversing the App_Plugins directory
            foreach (var subDir in GetToursFolderPaths(fileProvider, pluginFolderPath, "backoffice"))
            {
                IEnumerable<IFileInfo> tourFiles = fileProvider
                    .GetDirectoryContents(subDir)
                    .Where(x => x.Name.InvariantEndsWith(".json"));

                foreach (IFileInfo file in tourFiles)
                {
                    yield return new Tuple<IFileInfo, string>(file, pluginFolder.Name);
                }
            }
        }
    }

    private static IEnumerable<string> GetToursFolderPaths(IFileProvider fileProvider, string path, string subDirName)
    {
        // Hard-coding the "tours" directory name to gain a better performance when traversing the sub directories
        const string toursDirName = "tours";

        // It is necessary to iterate through the subfolders because on Linux we'll get casing issues when
        // we try to access {path}/{pluginDirectory.Name}/backoffice/tours directly
        foreach (IFileInfo subDir in fileProvider.GetDirectoryContents(path))
        {
            // InvariantEquals({dirName}) is used to gain a better performance when looking for the tours folder
            if (subDir.IsDirectory && subDir.Name.InvariantEquals(subDirName))
            {
                var virtualPath = WebPath.Combine(path, subDir.Name);

                if (subDir.Name.InvariantEquals(toursDirName))
                {
                    yield return virtualPath;
                }

                foreach (var nested in GetToursFolderPaths(fileProvider, virtualPath, toursDirName))
                {
                    yield return nested;
                }
            }
        }
    }

    /// <summary>
    ///     Gets a tours for a specific doctype.
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
        Stream fileStream,
        string? pluginName = null)
    {
        var fileName = Path.GetFileNameWithoutExtension(tourFile);
        if (fileName == null)
        {
            return;
        }

        // Get the filters specific to this file
        var fileFilters = filters.Where(x => x.TourFileName != null && x.TourFileName.IsMatch(fileName)).ToList();

        // If there is any filter applied to match the file only (no tour alias) then ignore the file entirely
        var isFileFiltered = fileFilters.Any(x => x.TourAlias == null);
        if (isFileFiltered)
        {
            return;
        }

        // Now combine all aliases to filter below
        var aliasFilters = aliasOnlyFilters.Concat(filters.Where(x => x.TourAlias != null))
            .Select(x => x.TourAlias)
            .ToList();

        try
        {
            using var reader = new StreamReader(fileStream, Encoding.UTF8);
            var contents = reader.ReadToEnd();
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

            // Don't add if all of the tours are filtered
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
