using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.AspNetCore;

public class AspNetCoreRequestAccessor : IRequestAccessor, INotificationHandler<UmbracoRequestBeginNotification>
{
    private readonly ISet<string> _applicationUrls = new HashSet<string>();
    private readonly IHttpContextAccessor _httpContextAccessor;
    private Uri? _currentApplicationUrl;
    private bool _hasAppUrl;
    private object _initLocker = new();
    private bool _isInit;
    private WebRoutingSettings _webRoutingSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AspNetCoreRequestAccessor" /> class.
    /// </summary>
    public AspNetCoreRequestAccessor(
        IHttpContextAccessor httpContextAccessor,
        IOptionsMonitor<WebRoutingSettings> webRoutingSettings)
    {
        _httpContextAccessor = httpContextAccessor;
        _webRoutingSettings = webRoutingSettings.CurrentValue;
        webRoutingSettings.OnChange(x => _webRoutingSettings = x);
    }

    /// <summary>
    ///     This just initializes the application URL on first request attempt
    ///     TODO: This doesn't belong here, the GetApplicationUrl doesn't belong to IRequestAccessor
    ///     this should be part of middleware not a lazy init based on an INotification
    /// </summary>
    public void Handle(UmbracoRequestBeginNotification notification)
        => LazyInitializer.EnsureInitialized(ref _hasAppUrl, ref _isInit, ref _initLocker, () =>
        {
            GetApplicationUrl();
            return true;
        });

    /// <inheritdoc />
    public string GetRequestValue(string name) => GetFormValue(name) ?? GetQueryStringValue(name);

    /// <inheritdoc />
    public string GetQueryStringValue(string name) => _httpContextAccessor.GetRequiredHttpContext().Request.Query[name];

    /// <inheritdoc />
    public Uri? GetRequestUrl() => _httpContextAccessor.HttpContext != null
        ? new Uri(_httpContextAccessor.HttpContext.Request.GetEncodedUrl())
        : null;

    public Uri? GetApplicationUrl()
    {
        // Fixme: This causes problems with site swap on azure because azure pre-warms a site by calling into `localhost` and when it does that
        // it changes the URL to `localhost:80` which actually doesn't work for pinging itself, it only works internally in Azure. The ironic part
        // about this is that this is here specifically for the slot swap scenario https://issues.umbraco.org/issue/U4-10626

        // see U4-10626 - in some cases we want to reset the application url
        // (this is a simplified version of what was in 7.x)
        // note: should this be optional? is it expensive?
        if (!(_webRoutingSettings.UmbracoApplicationUrl is null))
        {
            return new Uri(_webRoutingSettings.UmbracoApplicationUrl);
        }

        HttpRequest? request = _httpContextAccessor.HttpContext?.Request;

        if (request is null)
        {
            return _currentApplicationUrl;
        }

        var url = UriHelper.BuildAbsolute(request.Scheme, request.Host);
        if (!_applicationUrls.Contains(url))
        {
            _applicationUrls.Add(url);

            _currentApplicationUrl ??= new Uri(url);
        }

        return _currentApplicationUrl;
    }

    private string? GetFormValue(string name)
    {
        HttpRequest request = _httpContextAccessor.GetRequiredHttpContext().Request;
        if (!request.HasFormContentType)
        {
            return null;
        }

        return request.Form[name];
    }
}
