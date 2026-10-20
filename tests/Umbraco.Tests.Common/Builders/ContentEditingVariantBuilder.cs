using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ContentEditingVariantBuilder<TParent>(TParent parentBuilder)
    : ChildBuilderBase<TParent, VariantModel>(parentBuilder), IWithCultureBuilder, IWithNameBuilder
{
    private string? _culture;
    private string _name;

    string? IWithCultureBuilder.Culture
    {
        get => _culture;
        set => _culture = value;
    }

    string IWithNameBuilder.Name
    {
        get => _name;
        set => _name = value;
    }

    public override VariantModel Build() =>
        new()
        {
            Culture = _culture,
            Name = _name
        };
}
