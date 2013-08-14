using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.Date, "Date", "datepicker", ValueType = "DATE")]
    public class DatePropertyEditor : PropertyEditor
    {
        protected override ValueEditor CreateValueEditor()
        {
            var editor = base.CreateValueEditor();

            editor.Validators = new List<ValidatorBase> { new DateTimeValidator() };

            return editor;
        }
    }
}