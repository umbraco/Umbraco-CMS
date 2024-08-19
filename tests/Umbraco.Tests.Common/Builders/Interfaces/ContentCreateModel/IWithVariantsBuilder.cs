using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Tests.Common.Builders.Interfaces.ContentCreateModel;

public interface IWithVariantsBuilder
{
    public IEnumerable<VariantModel> Variants { get; set; }
}
