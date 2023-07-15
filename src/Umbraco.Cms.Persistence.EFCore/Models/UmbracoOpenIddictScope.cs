using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Models;

public class UmbracoOpenIddictScope
{
    public string Id { get; set; } = null!;

    public string? ConcurrencyToken { get; set; }

    public string? Description { get; set; }

    public string? Descriptions { get; set; }

    public string? DisplayName { get; set; }

    public string? DisplayNames { get; set; }

    public string? Name { get; set; }

    public string? Properties { get; set; }

    public string? Resources { get; set; }
}
