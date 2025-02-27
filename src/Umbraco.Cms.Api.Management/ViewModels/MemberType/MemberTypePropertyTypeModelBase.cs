using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.MemberType;

public abstract class MemberTypePropertyTypeModelBase : PropertyTypeModelBase
{
    public bool IsSensitive { get; set; }

    public MemberTypePropertyTypeVisibility Visibility { get; set; } = new();
}
