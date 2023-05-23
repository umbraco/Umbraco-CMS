using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class VariantTreeItemViewModel
{
    public required string Name { get; set; }

    public string? Culture { get; set; }

    public required PublishedState State { get; set; }
}
