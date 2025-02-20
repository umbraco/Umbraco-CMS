using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.AspNetCore;

public class AspNetCoreRequestAccessor : IRequestAccessor, IDisposable
{
    private readonly ISet<string> _applicationUrls = new HashSet<string>();
    private readonly IHttpContextAccessor _httpContextAccessor;
    private Uri? _currentApplicationUrl;
    private bool _hasAppUrl;
    private object _initLocker = new();
    private bool _isInit;
    private WebRoutingSettings _webRoutingSettings;
    private readonly IDisposable? _onChangeDisposable;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AspNetCoreRequestAccessor" /> class.
    /// </summary>
    public AspNetCoreRequestAccessor(
        IHttpContextAccessor httpContextAccessor,
        IOptionsMonitor<WebRoutingSettings> webRoutingSettings)
    {
        _httpContextAccessor = httpContextAccessor;
        _webRoutingSettings = webRoutingSettings.CurrentValue;
        _onChangeDisposable = webRoutingSettings.OnChange(x => _webRoutingSettings = x);
    }

    /// <inheritdoc />
    public string? GetRequestValue(string name) => GetFormValue(name) ?? GetQueryStringValue(name);

    /// <inheritdoc />
    public string? GetQueryStringValue(string name) => _httpContextAccessor.HttpContext?.Request.Query[name];

    /// <inheritdoc />
    public Uri? GetRequestUrl() => _httpContextAccessor.HttpContext != null
        ? new Uri(_httpContextAccessor.HttpContext.Request.GetEncodedUrl())
        : null;

    /// <summary>
    /// Ensure that the ApplicationUrl is set on the first request, this is using a LazyInitializer, so the code will only be run the first time
    /// </summary>
    /// TODO: This doesn't belong here, the GetApplicationUrl doesn't belong to IRequestAccessor
    internal void EnsureApplicationUrl() =>
        LazyInitializer.EnsureInitialized(ref _hasAppUrl, ref _isInit, ref _initLocker, () =>
        {
            GetApplicationUrl();
            return true;
        });

    public Uri? GetApplicationUrl()
    {
        // TODO: This causes problems with site swap on azure because azure pre-warms a site by calling into `localhost` and when it does that
        // it changes the URL to `localhost:80` which actually doesn't work for pinging itself, it only works internally in Azure. The ironic part
        // about this is that this is here specifically for the slot swap scenario https://issues.umbraco.org/issue/U4-10626

        // see U4-10626 - in some cases we want to reset the application url
        // (this is a simplified version of what was in 7.x)
        // note: should this be optional? is it expensive?
        if (_webRoutingSettings.UmbracoApplicationUrl is not null)
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
        HttpRequest? request = _httpContextAccessor.HttpContext?.Request;
        if (request?.HasFormContentType is not true)
        {
            return null;
        }

        return request.Form[name];
    }

    public void Dispose() => _onChangeDisposable?.Dispose();
}
