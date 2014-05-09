using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Core.Constants.PropertyEditors.GridAlias, "Grid", "grid", HideLabel=true,  IsParameterEditor = false, ValueType="JSON")]
    public class GridPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// Overridden to ensure that the value is validated
        /// </summary>
        /// <returns></returns>
        protected override PropertyValueEditor CreateValueEditor()
        {
            var editor = base.CreateValueEditor();
            
            return editor;
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new gridPreValueEditor();
        }

    }

    internal class gridPreValueEditor : PreValueEditor
    {
        [PreValueField("items", "Grid Configuration", "views/propertyeditors/grid/grid.prevalues.html", Description = "General grid configuration")]
        public string Items { get; set; }
    }
}
