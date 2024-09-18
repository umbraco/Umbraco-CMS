// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.HealthChecks.Checks.Security;

/// <summary>
///     Provides a base class for health checks of http header values.
/// </summary>
public abstract class BaseHttpHeaderCheck : HealthCheck
{
    private static HttpClient? _httpClient;
    private readonly string _header;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly string _localizedTextPrefix;
    private readonly bool _metaTagOptionAvailable;
    private readonly bool _shouldNotExist;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BaseHttpHeaderCheck" /> class.
    /// </summary>
    [Obsolete("Use constructor that takes all parameters instead.")]
    protected BaseHttpHeaderCheck(
        IHostingEnvironment hostingEnvironment,
        ILocalizedTextService textService,
        string header,
        string localizedTextPrefix,
        bool metaTagOptionAvailable)
        : this(hostingEnvironment, textService, header, localizedTextPrefix, metaTagOptionAvailable, false)
    { }

    protected BaseHttpHeaderCheck(
        IHostingEnvironment hostingEnvironment,
        ILocalizedTextService textService,
        string header,
        string localizedTextPrefix,
        bool metaTagOptionAvailable,
        bool shouldNotExist)
    {
        LocalizedTextService = textService ?? throw new ArgumentNullException(nameof(textService));
        _hostingEnvironment = hostingEnvironment;
        _header = header;
        _localizedTextPrefix = localizedTextPrefix;
        _metaTagOptionAvailable = metaTagOptionAvailable;
        _shouldNotExist = shouldNotExist;
    }

    [Obsolete("Save ILocalizedTextService in a field on the super class instead of using this")]
    protected ILocalizedTextService LocalizedTextService { get; }

    /// <summary>
    ///     Gets a link to an external read more page.
    /// </summary>
    protected abstract string ReadMoreLink { get; }

    private static HttpClient HttpClient => _httpClient ??= new HttpClient();

    /// <summary>
    ///     Get the status for this health check
    /// </summary>
    public override async Task<IEnumerable<HealthCheckStatus>> GetStatusAsync() =>
        await Task.WhenAll(CheckForHeader());

    /// <summary>
    ///     Executes the action and returns it's status
    /// </summary>
    public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        => throw new InvalidOperationException(
            "HTTP Header action requested is either not executable or does not exist");

    /// <summary>
    ///     The actual health check method.
    /// </summary>
    protected async Task<HealthCheckStatus> CheckForHeader()
    {
        string message;
        var success = false;
        StatusResultType resultType = StatusResultType.Warning;

        // Access the site home page and check for the click-jack protection header or meta tag
        var url = _hostingEnvironment.ApplicationMainUrl?.GetLeftPart(UriPartial.Authority);

        try
        {
            using HttpResponseMessage response = await HttpClient.GetAsync(url);

            // Check first for header
            success = HasMatchingHeader(response.Headers.Select(x => x.Key));

            // If not found, and available, check for meta-tag
            if (success == false && _metaTagOptionAvailable)
            {
                success = await DoMetaTagsContainKeyForHeader(response);
            }

            message = success
                ? LocalizedTextService.Localize("healthcheck", $"{_localizedTextPrefix}CheckHeaderFound")
                : LocalizedTextService.Localize("healthcheck", $"{_localizedTextPrefix}CheckHeaderNotFound");

            if (_shouldNotExist)
            {

                resultType = success ? StatusResultType.Error : StatusResultType.Success;
            }
            else
            {
                resultType = success ?  StatusResultType.Success : StatusResultType.Error;
            }
        }
        catch (Exception ex)
        {
            message = LocalizedTextService.Localize("healthcheck", "healthCheckInvalidUrl", new[] { url, ex.Message });
        }

        return
            new HealthCheckStatus(message)
            {
                ResultType = resultType,
                ReadMoreLink = success && !string.IsNullOrEmpty(ReadMoreLink) ? null : ReadMoreLink,
            };
    }

    private static Dictionary<string, string> ParseMetaTags(string html)
    {
        var regex = new Regex("<meta http-equiv=\"(.+?)\" content=\"(.+?)\"", RegexOptions.IgnoreCase);

        return regex.Matches(html)
            .ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value);
    }

    private bool HasMatchingHeader(IEnumerable<string> headerKeys)
        => headerKeys.Contains(_header, StringComparer.InvariantCultureIgnoreCase);

    private async Task<bool> DoMetaTagsContainKeyForHeader(HttpResponseMessage response)
    {
        using (Stream stream = await response.Content.ReadAsStreamAsync())
        {
            if (stream == null)
            {
                return false;
            }

            using (var reader = new StreamReader(stream))
            {
                var html = reader.ReadToEnd();
                Dictionary<string, string> metaTags = ParseMetaTags(html);
                return HasMatchingHeader(metaTags.Keys);
            }
        }
    }
}
