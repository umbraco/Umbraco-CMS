using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Examine
{
    public abstract class BaseValueSetBuilder 
    {
        private readonly PropertyEditorCollection _propertyEditors;

        public BaseValueSetBuilder(PropertyEditorCollection propertyEditors)
        {
            _propertyEditors = propertyEditors ?? throw new System.ArgumentNullException(nameof(propertyEditors));
        }

        protected void AddPropertyValue(Property property, string culture, string segment, IDictionary<string, object[]> values)
        {
            var editor = _propertyEditors[property.Alias];
            if (editor == null) return;

            var indexVals = editor.ValueIndexer.GetIndexValues(property, culture, segment);
            foreach (var keyVal in indexVals)
            {
                if (keyVal.Key.IsNullOrWhiteSpace()) continue;

                var cultureSuffix = culture == null ? string.Empty : "_" + culture;

                foreach (var val in keyVal.Value)
                {
                    switch (val)
                    {
                        //only add the value if its not null or empty (we'll check for string explicitly here too)
                        case null:
                            continue;
                        case string strVal:
                            if (strVal.IsNullOrWhiteSpace()) return;
                            values.Add($"{keyVal.Key}{cultureSuffix}", new[] { val });
                            break;
                        default:
                            values.Add($"{keyVal.Key}{cultureSuffix}", new[] { val });
                            break;
                    }
                }
            }


        }
    }

}
