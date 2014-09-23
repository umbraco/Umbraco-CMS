using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Core.Constants.PropertyEditors.CanvasAlias, "Canvas", "canvas", HideLabel=true,  IsParameterEditor = false, ValueType="JSON")]
    public class CanvasPropertyEditor : PropertyEditor
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
            return new canvasPreValueEditor();
        }

    }

    internal class canvasPreValueEditor : PreValueEditor
    {
        [PreValueField("items", "Canvas", "views/propertyeditors/canvas/canvas.prevalues.html", Description = "Canvas configuration")]
        public string Items { get; set; }

        [PreValueField("rte", "Rich text editor", "views/propertyeditors/rte/rte.prevalues.html", Description = "Rich text editor configuration")]
        public string Rte { get; set; }
    }
}
