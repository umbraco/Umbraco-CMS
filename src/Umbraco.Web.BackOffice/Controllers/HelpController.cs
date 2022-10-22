using System.Net;
using System.Runtime.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
public class HelpController : UmbracoAuthorizedJsonController
{
    private static HttpClient? _httpClient;
    private readonly ILogger<HelpController> _logger;
    private HelpPageSettings? _helpPageSettings;

    [Obsolete("Use constructor that takes IOptions<HelpPageSettings>")]
    public HelpController(ILogger<HelpController> logger)
        : this(logger, StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<HelpPageSettings>>())
    {
    }

    [ActivatorUtilitiesConstructor]
    public HelpController(
        ILogger<HelpController> logger,
        IOptionsMonitor<HelpPageSettings> helpPageSettings)
    {
        _logger = logger;

        ResetHelpPageSettings(helpPageSettings.CurrentValue);
        helpPageSettings.OnChange(ResetHelpPageSettings);
    }

    private void ResetHelpPageSettings(HelpPageSettings settings) => _helpPageSettings = settings;

    public async Task<List<HelpPage>> GetContextHelpForPage(string section, string tree,
        string baseUrl = "https://our.umbraco.com")
    {
        if (IsAllowedUrl(baseUrl) is false)
        {
            _logger.LogError(
                $"The following URL is not listed in the allowlist for HelpPage in HelpPageSettings: {baseUrl}");
            HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            // Ideally we'd want to return a BadRequestResult here,
            // however, since we're not returning ActionResult this is not possible and changing it would be a breaking change.
            return new List<HelpPage>();
        }

        var url = string.Format(
            baseUrl + "/Umbraco/Documentation/Lessons/GetContextHelpDocs?sectionAlias={0}&treeAlias={1}", section,
            tree);

        try
        {
            if (_httpClient == null)
            {
                _httpClient = new HttpClient();
            }

            //fetch dashboard json and parse to JObject
            var json = await _httpClient.GetStringAsync(url);
            List<HelpPage>? result = JsonConvert.DeserializeObject<List<HelpPage>>(json);
            if (result != null)
            {
                return result;
            }
        }
        catch (HttpRequestException rex)
        {
            _logger.LogInformation($"Check your network connection, exception: {rex.Message}");
        }

        return new List<HelpPage>();
    }

    private bool IsAllowedUrl(string url)
    {
        if (_helpPageSettings?.HelpPageUrlAllowList is null ||
            _helpPageSettings.HelpPageUrlAllowList.Contains(url))
        {
            return true;
        }

        return false;
    }
}

[DataContract(Name = "HelpPage")]
public class HelpPage
{
    [DataMember(Name = "name")] public string? Name { get; set; }

    [DataMember(Name = "description")] public string? Description { get; set; }

    [DataMember(Name = "url")] public string? Url { get; set; }

    [DataMember(Name = "type")] public string? Type { get; set; }
}
