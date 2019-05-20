using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor("Umbraco.SimpleGrid", EditorType.PropertyValue , "Simple Grid", "simplegrid", ValueType = ValueTypes.Json, Group="rich content", Icon="icon-application-window-alt")]
    public class SimpleGridPropertyEditor : DataEditor
    {
        public SimpleGridPropertyEditor(ILogger logger)
            : base(logger)
        { }

        protected override IConfigurationEditor CreateConfigurationEditor() => new SimpleGridConfigurationEditor();

        protected override IDataValueEditor CreateValueEditor() => base.CreateValueEditor();
    }
}
