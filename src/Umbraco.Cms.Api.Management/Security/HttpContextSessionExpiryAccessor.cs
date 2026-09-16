using System.Globalization;
using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Api.Management.Security;

/// <summary>
///     Reads the back-office session expiry from the authentication ticket-expiry claim on the current request.
/// </summary>
/// <remarks>
///     The claim is written on every cookie validation - see
///     <see cref="Configuration.ConfigureBackOfficeCookieOptions" />. Note it is written before the ticket
///     expiry is reset, so on a renewing request it still carries the pre-renewal value and only catches up
///     on the next request.
/// </remarks>
public class HttpContextSessionExpiryAccessor : ISessionExpiryAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HttpContextSessionExpiryAccessor" /> class.
    /// </summary>
    /// <param name="httpContextAccessor">Accessor for the current HTTP context.</param>
    public HttpContextSessionExpiryAccessor(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    /// <inheritdoc />
    public DateTimeOffset? GetSessionExpiry()
    {
        var ticketExpires = _httpContextAccessor.HttpContext?.User.FindFirst(Constants.Security.TicketExpiresClaimType)?.Value;

        return DateTimeOffset.TryParse(ticketExpires, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTimeOffset expiry)
            ? expiry
            : null;
    }
}
