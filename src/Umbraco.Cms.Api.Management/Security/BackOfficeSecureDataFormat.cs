using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Security;

/// <summary>
///     Custom secure format that ensures the Identity in the ticket is verified <see cref="ClaimsIdentity" />
/// </summary>
internal sealed class BackOfficeSecureDataFormat : ISecureDataFormat<AuthenticationTicket>
{
    private readonly TimeSpan _loginTimeout;
    private readonly ISecureDataFormat<AuthenticationTicket> _ticketDataFormat;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackOfficeSecureDataFormat"/> class with the specified login timeout and authentication ticket data format.
    /// </summary>
    /// <param name="loginTimeout">The duration for which a login session remains valid.</param>
    /// <param name="ticketDataFormat">The secure data format used to serialize and deserialize authentication tickets.</param>
    public BackOfficeSecureDataFormat(TimeSpan loginTimeout, ISecureDataFormat<AuthenticationTicket> ticketDataFormat)
    {
        _loginTimeout = loginTimeout;
        _ticketDataFormat = ticketDataFormat ?? throw new ArgumentNullException(nameof(ticketDataFormat));
    }

    /// <summary>
    /// Protects the specified authentication ticket by creating a secured, serialized string representation.
    /// </summary>
    /// <param name="data">The authentication ticket to protect.</param>
    /// <param name="purpose">An optional purpose string that can be used to provide additional context for the protection operation.</param>
    /// <returns>A protected string that represents the secured authentication ticket.</returns>
    public string Protect(AuthenticationTicket data, string? purpose)
    {
        // create a new ticket based on the passed in tickets details, however, we'll adjust the expires utc based on the specified timeout mins
        var ticket = new AuthenticationTicket(
            data.Principal,
            new AuthenticationProperties(data.Properties.Items)
            {
                IssuedUtc = data.Properties.IssuedUtc,
                ExpiresUtc = data.Properties.ExpiresUtc ?? DateTimeOffset.UtcNow.Add(_loginTimeout),
                AllowRefresh = data.Properties.AllowRefresh,
                IsPersistent = data.Properties.IsPersistent,
                RedirectUri = data.Properties.RedirectUri
            },
            data.AuthenticationScheme);

        return _ticketDataFormat.Protect(ticket);
    }

    /// <summary>
    /// Protects the specified authentication ticket by creating a secured string representation.
    /// The method may adjust the ticket's expiration time based on the configured login timeout before protecting it.
    /// </summary>
    /// <param name="data">The authentication ticket to protect.</param>
    /// <returns>A protected string representing the authentication ticket.</returns>
    public string Protect(AuthenticationTicket data) => Protect(data, string.Empty);


    /// <summary>
    /// Unprotects the specified protected text and returns the corresponding <see cref="AuthenticationTicket"/>.
    /// </summary>
    /// <param name="protectedText">The protected text to unprotect.</param>
    /// <returns>The <see cref="AuthenticationTicket"/> if unprotection is successful; otherwise, null.</returns>
    public AuthenticationTicket? Unprotect(string? protectedText) => Unprotect(protectedText, string.Empty);

    /// <summary>
    ///     Un-protects the cookie
    /// </summary>
    /// <param name="protectedText"></param>
    /// <param name="purpose"></param>
    /// <returns></returns>
    public AuthenticationTicket? Unprotect(string? protectedText, string? purpose)
    {
        AuthenticationTicket? decrypt;
        try
        {
            decrypt = _ticketDataFormat.Unprotect(protectedText);
            if (decrypt == null)
            {
                return null;
            }
        }
        catch (Exception)
        {
            return null;
        }

        var identity = (ClaimsIdentity?)decrypt.Principal.Identity;
        if (identity is null || !identity.VerifyBackOfficeIdentity(out ClaimsIdentity? verifiedIdentity))
        {
            return null;
        }

        //return the ticket with a UmbracoBackOfficeIdentity
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(verifiedIdentity), decrypt.Properties, decrypt.AuthenticationScheme);

        return ticket;
    }
}
