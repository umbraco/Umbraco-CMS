using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Tests.Common.Builders.Interfaces.ContentCreateModel;

public interface IWithInvariantPropertiesBuilder
{
    public IEnumerable<PropertyValueModel> InvariantProperties { get; set; }
}
