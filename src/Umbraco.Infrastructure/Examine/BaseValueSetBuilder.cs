using Examine;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <inheritdoc />
public abstract class BaseValueSetBuilder<TContent> : IValueSetBuilder<TContent>
    where TContent : IContentBase
{
    private readonly PropertyEditorCollection _propertyEditors;

    protected BaseValueSetBuilder(PropertyEditorCollection propertyEditors, bool publishedValuesOnly)
    {
        PublishedValuesOnly = publishedValuesOnly;
        _propertyEditors = propertyEditors ?? throw new ArgumentNullException(nameof(propertyEditors));
    }

    protected bool PublishedValuesOnly { get; }

    /// <inheritdoc />
    public abstract IEnumerable<ValueSet> GetValueSets(params TContent[] content);

    protected void AddPropertyValue(IProperty property, string? culture, string? segment, IDictionary<string, IEnumerable<object?>>? values, IEnumerable<string> availableCultures, IDictionary<Guid, IContentType> contentTypeDictionary)
    {
        IDataEditor? editor = _propertyEditors[property.PropertyType.PropertyEditorAlias];
        if (editor == null)
        {
            return;
        }

        IEnumerable<IndexValue> indexVals =
            editor.PropertyIndexValueFactory.GetIndexValues(property, culture, segment, PublishedValuesOnly, availableCultures, contentTypeDictionary);
        foreach (IndexValue indexValue in indexVals)
        {
            if (indexValue.FieldName.IsNullOrWhiteSpace())
            {
                continue;
            }

            var indexValueCulture = indexValue.Culture ?? culture;
            var cultureSuffix = indexValueCulture == null ? string.Empty : "_" + indexValueCulture.ToLowerInvariant();

            foreach (var val in indexValue.Values)
            {
                switch (val)
                {
                    // only add the value if its not null or empty (we'll check for string explicitly here too)
                    case null:
                        continue;
                    case string strVal:
                        {
                        if (strVal.IsNullOrWhiteSpace())
                        {
                            continue;
                        }

                        var key = $"{indexValue.FieldName}{cultureSuffix}";
                        if (values?.TryGetValue(key, out IEnumerable<object?>? v) ?? false)
                        {
                            values[key] = new List<object?>(v) { val }.ToArray();
                        }
                        else
                        {
                            values?.Add($"{indexValue.FieldName}{cultureSuffix}", val.Yield());
                        }
                    }

                        break;
                    default:
                        {
                        var key = $"{indexValue.FieldName}{cultureSuffix}";
                        if (values?.TryGetValue(key, out IEnumerable<object?>? v) ?? false)
                        {
                            values[key] = new List<object?>(v) { val }.ToArray();
                        }
                        else
                        {
                            values?.Add($"{indexValue.FieldName}{cultureSuffix}", val.Yield());
                        }
                    }

                        break;
                }
            }
        }
    }
}
