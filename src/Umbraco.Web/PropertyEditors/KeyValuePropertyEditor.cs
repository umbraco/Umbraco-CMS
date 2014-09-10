using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.KeyValueListAlias, "Key/Value List", "keyvaluelist", ValueType = "TEXT")]
    public class KeyValuePropertyEditor : PropertyEditor
    {
        protected override PreValueEditor CreatePreValueEditor()
        {
            return new KeyValuePreValueEditor();
        }

        /// <summary>
        /// A custom pre-value editor class to deal with the legacy way that the pre-value data is stored.
        /// </summary>
        internal class KeyValuePreValueEditor : PreValueEditor
        {
            public KeyValuePreValueEditor()
            {
                //create the fields
                Fields.Add(new PreValueField(new IntegerValidator())
                {
                    Description = "Enter the minimum amount of text boxes to be displayed",
                    Key = "min",
                    View = "requiredfield",
                    Name = "Minimum"
                });
                Fields.Add(new PreValueField(new IntegerValidator())
                {
                    Description = "Enter the maximum amount of text boxes to be displayed, enter 0 for unlimited",
                    Key = "max",
                    View = "requiredfield",
                    Name = "Maximum"
                });
            }

        }
    }
}
