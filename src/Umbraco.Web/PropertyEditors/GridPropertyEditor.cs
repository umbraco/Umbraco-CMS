using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Core.Constants.PropertyEditors.GridAlias, "Grid layout", "grid", HideLabel = true, IsParameterEditor = false, ValueType = "JSON")]
    public class GridPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// Overridden to ensure that the value is validated
        /// </summary>
        /// <returns></returns>
        protected override PropertyValueEditor CreateValueEditor()
        {
            var baseEditor = base.CreateValueEditor();
            return new GridPropertyValueEditor(baseEditor);
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new GridPreValueEditor();
        }

        internal class GridPropertyValueEditor : PropertyValueEditorWrapper
        {
            public GridPropertyValueEditor(PropertyValueEditor wrapped)
                : base(wrapped)
            {
            }

        }

        internal class GridPreValueEditor : PreValueEditor
        {
            [PreValueField("items", "Grid", "views/propertyeditors/grid/grid.prevalues.html", Description = "Grid configuration")]
            public string Items { get; set; }

            [PreValueField("rte", "Rich text editor", "views/propertyeditors/rte/rte.prevalues.html", Description = "Rich text editor configuration")]
            public string Rte { get; set; }
        }
    }


}
