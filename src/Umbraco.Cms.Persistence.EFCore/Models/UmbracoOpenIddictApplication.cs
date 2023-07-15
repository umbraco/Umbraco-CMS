using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Models;

public class UmbracoOpenIddictApplication
{
    public string Id { get; set; } = null!;

    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    public string? ConcurrencyToken { get; set; }

    public string? ConsentType { get; set; }

    public string? DisplayName { get; set; }

    public string? DisplayNames { get; set; }

    public string? Permissions { get; set; }

    public string? PostLogoutRedirectUris { get; set; }

    public string? Properties { get; set; }

    public string? RedirectUris { get; set; }

    public string? Requirements { get; set; }

    public string? Type { get; set; }

    public virtual ICollection<UmbracoOpenIddictAuthorization> UmbracoOpenIddictAuthorizations { get; set; } = new List<UmbracoOpenIddictAuthorization>();

    public virtual ICollection<UmbracoOpenIddictToken> UmbracoOpenIddictTokens { get; set; } = new List<UmbracoOpenIddictToken>();
}
