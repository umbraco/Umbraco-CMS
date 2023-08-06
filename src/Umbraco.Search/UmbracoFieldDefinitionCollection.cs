using System.Collections;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Search;
using Umbraco.Search.Enums;
using Umbraco.Search.Extensions;

namespace Umbraco.Search;

public class UmbracoFieldDefinitionCollection : IEnumerable<FieldDefinition>
{
    public UmbracoFieldDefinitionCollection()
        : this(Enumerable.Empty<FieldDefinition>())
    {
    }

    public UmbracoFieldDefinitionCollection(params FieldDefinition[] definitions)
        : this((IEnumerable<FieldDefinition>)definitions)
    {
    }


    /// <summary>
    ///     A type that defines the type of index for each Umbraco field (non user defined fields)
    ///     Alot of standard umbraco fields shouldn't be tokenized or even indexed, just stored into lucene
    ///     for retreival after searching.
    /// </summary>
    public static readonly FieldDefinition[] UmbracoIndexFieldDefinitions =
    {
        new(UmbracoSearchFieldNames.ParentID, UmbracoFieldType.Integer), new("level", UmbracoFieldType.Integer),
        new("writerID", UmbracoFieldType.Integer), new("creatorID", UmbracoFieldType.Integer),
        new("sortOrder", UmbracoFieldType.Integer), new("template", UmbracoFieldType.Integer),
        new("createDate", UmbracoFieldType.DateTime), new("updateDate", UmbracoFieldType.DateTime),
        new(UmbracoSearchFieldNames.NodeKeyFieldName, UmbracoFieldType.InvariantCultureIgnoreCase),
        new("version", UmbracoFieldType.Raw), new("nodeType", UmbracoFieldType.InvariantCultureIgnoreCase),
        new("template", UmbracoFieldType.Raw), new("urlName", UmbracoFieldType.InvariantCultureIgnoreCase),
        new("path", UmbracoFieldType.Raw), new("email", UmbracoFieldType.EmailAddress),
        new(UmbracoSearchFieldNames.PublishedFieldName, UmbracoFieldType.Raw),
        new(UmbracoSearchFieldNames.IndexPathFieldName, UmbracoFieldType.Raw),
        new(UmbracoSearchFieldNames.IconFieldName, UmbracoFieldType.Raw),
        new(UmbracoSearchFieldNames.VariesByCultureFieldName, UmbracoFieldType.Raw),
    };

    public UmbracoFieldDefinitionCollection(IEnumerable<FieldDefinition> definitions)
    {
        foreach (var f in definitions.GroupBy(x => x.Name))
        {
            var indexField = f.FirstOrDefault();
            if (indexField is null)
            {
                continue;
            }

            Definitions.TryAdd(f.Key, indexField);
        }

        foreach (var f in UmbracoIndexFieldDefinitions.GroupBy(x => x.Name))
        {
            var indexField = f.FirstOrDefault();
            if (indexField is null)
            {
                continue;
            }

            Definitions.TryAdd(f.Key, indexField);
        }
    }

    /// <summary>
    ///     Overridden to dynamically add field definitions for culture variations
    /// </summary>
    /// <param name="fieldName"></param>
    /// <param name="fieldDefinition"></param>
    /// <returns></returns>
    /// <remarks>
    ///     We need to do this so that we don't have to maintain a huge static list of all field names and their definitions
    ///     otherwise we'd have to dynamically add/remove definitions anytime languages are added/removed, etc...
    ///     For example, we have things like `nodeName` and `__Published` which are also used for culture fields like
    ///     `nodeName_en-us`
    ///     and we don't want to have a full static list of all of these definitions when we can just define the one definition
    ///     and then
    ///     dynamically apply that to culture specific fields.
    ///     There is a caveat to this however, when a field definition is found for a non-culture field we will create and
    ///     store a new field
    ///     definition for that culture so that the next time it needs to be looked up and used we are not allocating more
    ///     objects. This does mean
    ///     however that if a language is deleted, the field definitions for that language will still exist in memory. This
    ///     isn't going to cause any
    ///     problems and the mem will be cleared on next site restart but it's worth pointing out.
    /// </remarks>
    public bool TryGetValue(string fieldName, out FieldDefinition? fieldDefinition)
    {
        fieldDefinition = null;
        // before we use regex to match do some faster simple matching since this is going to execute quite a lot
        if (!fieldName.Contains("_"))
        {
            return false;
        }

        Match match = UmbracoSearchExtensions._cultureIsoCodeFieldNameMatchExpression.Match(fieldName);
        if (match.Success)
        {
            var nonCultureFieldName = match.Groups["FieldName"].Value;

            // check if there's a definition for this and if so return the field definition for the culture field based on the non-culture field
            if (Definitions.TryGetValue(fieldName, out var existingFieldDefinition))
            {
                // now add a new field def
                fieldDefinition = GetOrAdd(fieldName, s => new FieldDefinition(s, existingFieldDefinition.Type));
                return true;
            }
        }

        return false;
    }

    protected ConcurrentDictionary<string, FieldDefinition> Definitions { get; } =
        new ConcurrentDictionary<string, FieldDefinition>(StringComparer.InvariantCultureIgnoreCase);

    public IEnumerator<FieldDefinition> GetEnumerator() => Definitions.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public FieldDefinition GetOrAdd(string fieldName, Func<string, FieldDefinition> add) =>
        Definitions.GetOrAdd(fieldName, add);

    public void AddOrUpdate(FieldDefinition definition) =>
        Definitions.AddOrUpdate(definition.Name, definition, (s, factory) => definition);

    public bool TryAdd(FieldDefinition definition) => Definitions.TryAdd(definition.Name, definition);
}
