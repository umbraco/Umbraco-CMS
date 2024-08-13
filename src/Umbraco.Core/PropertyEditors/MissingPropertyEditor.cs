using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a temporary representation of an editor for cases where a data type is created but not editor is
///     available.
/// </summary>
[HideFromTypeFinder]
public class MissingPropertyEditor : IDataEditor
{
    public string Alias => "Umbraco.Missing";

    public string Name => "Missing property editor";

    public bool IsDeprecated => false;

    public IDictionary<string, object> DefaultConfiguration => throw new NotImplementedException();

    public IPropertyIndexValueFactory PropertyIndexValueFactory => throw new NotImplementedException();

    public IConfigurationEditor GetConfigurationEditor() => new ConfigurationEditor();

    public IDataValueEditor GetValueEditor() => throw new NotImplementedException();

    public IDataValueEditor GetValueEditor(object? configurationObject) => throw new NotImplementedException();
}
