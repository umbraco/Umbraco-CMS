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
    private static HttpClient? httpClient;
    private readonly string _header;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly string _localizedTextPrefix;
    private readonly bool _metaTagOptionAvailable;

    [Obsolete("Use ctor without value.")]
    protected BaseHttpHeaderCheck(
        IHostingEnvironment hostingEnvironment,
        ILocalizedTextService textService,
        string header,
        string value,
        string localizedTextPrefix,
        bool metaTagOptionAvailable)
        : this(hostingEnvironment, textService, header, localizedTextPrefix, metaTagOptionAvailable)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BaseHttpHeaderCheck" /> class.
    /// </summary>
    protected BaseHttpHeaderCheck(
        IHostingEnvironment hostingEnvironment,
        ILocalizedTextService textService,
        string header,
        string localizedTextPrefix,
        bool metaTagOptionAvailable)
    {
        LocalizedTextService = textService ?? throw new ArgumentNullException(nameof(textService));
        _hostingEnvironment = hostingEnvironment;
        _header = header;
        _localizedTextPrefix = localizedTextPrefix;
        _metaTagOptionAvailable = metaTagOptionAvailable;
    }

    [Obsolete("Save ILocalizedTextService in a field on the super class instead of using this")]
    protected ILocalizedTextService LocalizedTextService { get; }

    /// <summary>
    ///     Gets a link to an external read more page.
    /// </summary>
    protected abstract string ReadMoreLink { get; }

    private static HttpClient HttpClient => httpClient ??= new HttpClient();

    /// <summary>
    ///     Get the status for this health check
    /// </summary>
    public override async Task<IEnumerable<HealthCheckStatus>> GetStatus() =>
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
        }
        catch (Exception ex)
        {
            message = LocalizedTextService.Localize("healthcheck", "healthCheckInvalidUrl", new[] { url, ex.Message });
        }

        return
            new HealthCheckStatus(message)
            {
                ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                ReadMoreLink = success ? null : ReadMoreLink,
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
