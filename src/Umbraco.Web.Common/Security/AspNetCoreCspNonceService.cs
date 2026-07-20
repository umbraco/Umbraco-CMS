using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Security;
using CoreConstants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.Common.Security;

/// <summary>
/// ASP.NET Core implementation of <see cref="ICspNonceService"/> that generates and caches
/// a cryptographically secure nonce per HTTP request.
/// </summary>
public class AspNetCoreCspNonceService : ICspNonceService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="AspNetCoreCspNonceService"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public AspNetCoreCspNonceService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    /// <inheritdoc />
    public string? GetNonce()
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return null;
        }

        if (httpContext.Items.TryGetValue(CoreConstants.HttpContext.Items.CspNonce, out var existing) && existing is string nonce)
        {
            return nonce;
        }

        var newNonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        httpContext.Items[CoreConstants.HttpContext.Items.CspNonce] = newNonce;
        return newNonce;
    }
}
