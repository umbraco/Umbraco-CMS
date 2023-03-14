using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Services;

public interface IEditorConfigurationParser
{
    TConfiguration? ParseFromConfigurationEditor<TConfiguration>(
        IDictionary<string, object?>? editorValues,
        IEnumerable<ConfigurationField> fields);

    Dictionary<string, object> ParseToConfigurationEditor<TConfiguration>(TConfiguration? configuration);
}
