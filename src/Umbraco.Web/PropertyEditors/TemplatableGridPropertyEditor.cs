using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Core.Constants.PropertyEditors.TemplatableGridAlias, "Templatable grid layout", "templatablegrid", HideLabel = true, IsParameterEditor = false, ValueType = "JSON", Group = "rich content", Icon = "icon-layout")]
    public class TemplatableGridPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// Overridden to ensure that the value is validated
        /// </summary>
        /// <returns></returns>
        protected override PropertyValueEditor CreateValueEditor()
        {
            var baseEditor = base.CreateValueEditor();
            return new TemplatableGridPropertyValueEditor(baseEditor);
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new TemplatableGridPreValueEditor();
        }

        internal class TemplatableGridPropertyValueEditor : PropertyValueEditorWrapper
        {
            public TemplatableGridPropertyValueEditor(PropertyValueEditor wrapped)
                : base(wrapped)
            {
            }

        }

        internal class TemplatableGridPreValueEditor : PreValueEditor
        {
            [PreValueField("items", "Grid", "views/propertyeditors/grid/grid.prevalues.html", Description = "Grid configuration")]
            public string Items { get; set; }

            [PreValueField("rte", "Rich text editor", "views/propertyeditors/rte/rte.prevalues.html", Description = "Rich text editor configuration")]
            public string Rte { get; set; }

            [PreValueField("template", "Template", "views/propertyeditors/templatablegrid/template.prevalues.html", Description = "Grid template")]
            public string DefaultLayout { get; set; }
        }
    }
}
