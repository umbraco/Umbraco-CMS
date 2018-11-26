using System.Linq;
using Umbraco.Core.Logging;
using Examine;
using Lucene.Net.Documents;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    using Examine = global::Examine;

    /// <summary>
    /// Represents a grid property and parameter editor.
    /// </summary>
    [DataEditor(Constants.PropertyEditors.Aliases.Grid, "Grid layout", "grid", HideLabel = true, ValueType = ValueTypes.Json, Group="rich content", Icon="icon-layout")]
    public class GridPropertyEditor : DataEditor
    {
        public GridPropertyEditor(ILogger logger)
            : base(logger)
        { }

        public override IValueIndexer ValueIndexer => new GridValueIndexer();

        /// <summary>
        /// Overridden to ensure that the value is validated
        /// </summary>
        /// <returns></returns>
        protected override IDataValueEditor CreateValueEditor() => new GridPropertyValueEditor(Attribute);

        protected override IConfigurationEditor CreateConfigurationEditor() => new GridConfigurationEditor();

        internal class GridPropertyValueEditor : DataValueEditor
        {
            public GridPropertyValueEditor(DataEditorAttribute attribute)
                : base(attribute)
            { }
        }
    }
}
