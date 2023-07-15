using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Models;

public class UmbracoOpenIddictAuthorization
{
    public string Id { get; set; } = null!;

    public string? ApplicationId { get; set; }

    public string? ConcurrencyToken { get; set; }

    public DateTime? CreationDate { get; set; }

    public string? Properties { get; set; }

    public string? Scopes { get; set; }

    public string? Status { get; set; }

    public string? Subject { get; set; }

    public string? Type { get; set; }

    public virtual UmbracoOpenIddictApplication? Application { get; set; }

    public virtual ICollection<UmbracoOpenIddictToken> UmbracoOpenIddictTokens { get; set; } = new List<UmbracoOpenIddictToken>();
}
