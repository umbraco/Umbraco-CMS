// Copyright (c) Umbraco.
// See LICENSE for more details.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     A handler for NestedContent used to bind to notifications
/// </summary>
public class NestedContentPropertyHandler : ComplexPropertyEditorContentNotificationHandler
{
    protected override string EditorAlias => Constants.PropertyEditors.Aliases.NestedContent;

    // internal for tests
    internal string CreateNestedContentKeys(string rawJson, bool onlyMissingKeys, Func<Guid>? createGuid = null)
    {
        // used so we can test nicely
        if (createGuid == null)
        {
            createGuid = () => Guid.NewGuid();
        }

        if (string.IsNullOrWhiteSpace(rawJson) || !rawJson.DetectIsJson())
        {
            return rawJson;
        }

        // Parse JSON
        var complexEditorValue = JToken.Parse(rawJson);

        UpdateNestedContentKeysRecursively(complexEditorValue, onlyMissingKeys, createGuid);

        return complexEditorValue.ToString(Formatting.None);
    }

    protected override string FormatPropertyValue(string rawJson, bool onlyMissingKeys) =>
        CreateNestedContentKeys(rawJson, onlyMissingKeys);

    private void UpdateNestedContentKeysRecursively(JToken json, bool onlyMissingKeys, Func<Guid> createGuid)
    {
        // check if this is NC
        var isNestedContent =
            json.SelectTokens($"$..['{NestedContentPropertyEditor.ContentTypeAliasPropertyKey}']", false).Any();

        // select all values (flatten)
        var allProperties = json.SelectTokens("$..*").OfType<JValue>().Select(x => x.Parent as JProperty).WhereNotNull()
            .ToList();
        foreach (JProperty prop in allProperties)
        {
            if (prop.Name == NestedContentPropertyEditor.ContentTypeAliasPropertyKey)
            {
                // get it's sibling 'key' property
                var ncKeyVal = prop.Parent?["key"] as JValue;
                if ((onlyMissingKeys && ncKeyVal == null) || (!onlyMissingKeys && ncKeyVal != null))
                {
                    // create or replace
                    prop.Parent!["key"] = createGuid().ToString();
                }
            }
            else if (!isNestedContent || prop.Name != "key")
            {
                // this is an arbitrary property that could contain a nested complex editor
                var propVal = prop.Value.ToString();

                // check if this might contain a nested NC
                if (!propVal.IsNullOrWhiteSpace() && propVal.DetectIsJson() &&
                    propVal.InvariantContains(NestedContentPropertyEditor.ContentTypeAliasPropertyKey))
                {
                    // recurse
                    var parsed = JToken.Parse(propVal);
                    UpdateNestedContentKeysRecursively(parsed, onlyMissingKeys, createGuid);

                    // set the value to the updated one
                    prop.Value = parsed.ToString(Formatting.None);
                }
            }
        }
    }
}
