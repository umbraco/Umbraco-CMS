using System.Collections.Generic;
using System.Linq;
using Examine;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Examine
{

    /// <inheritdoc />
    public abstract class BaseValueSetBuilder<TContent> : IValueSetBuilder<TContent>
        where TContent : IContentBase
    {
        private readonly PropertyEditorCollection _propertyEditors;

        protected BaseValueSetBuilder(PropertyEditorCollection propertyEditors)
        {
            _propertyEditors = propertyEditors ?? throw new System.ArgumentNullException(nameof(propertyEditors));
        }

        /// <inheritdoc />
        public abstract IEnumerable<ValueSet> GetValueSets(params TContent[] content);

        protected void AddPropertyValue(Property property, string culture, string segment, IDictionary<string, IEnumerable<object>> values)
        {
            var editor = _propertyEditors[property.PropertyType.PropertyEditorAlias];
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
                            {
                                if (strVal.IsNullOrWhiteSpace()) return;
                                var key = $"{keyVal.Key}{cultureSuffix}";
                                if (values.TryGetValue(key, out var v))
                                    values[key] = new List<object>(v) { val }.ToArray();
                                else
                                    values.Add($"{keyVal.Key}{cultureSuffix}", val.Yield());
                            }
                            break;
                        default:
                            {
                                var key = $"{keyVal.Key}{cultureSuffix}";
                                if (values.TryGetValue(key, out var v))
                                    values[key] = new List<object>(v) { val }.ToArray();
                                else
                                    values.Add($"{keyVal.Key}{cultureSuffix}", val.Yield());
                            }
                            
                            break;
                    }
                }
            }
        }
    }

}
