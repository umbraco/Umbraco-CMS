using System.Collections.Generic;
using System.Text.RegularExpressions;
using Examine;
using Umbraco.Core;

namespace Umbraco.Examine
{
    /// <summary>
    /// Custom <see cref="FieldDefinitionCollection"/> allowing dynamic creation of <see cref="FieldDefinition"/>
    /// </summary>
    public class UmbracoFieldDefinitionCollection : FieldDefinitionCollection
    {
        
        public UmbracoFieldDefinitionCollection()
            : base(UmbracoIndexFieldDefinitions)
        {
        }

        /// <summary>
        /// A type that defines the type of index for each Umbraco field (non user defined fields)
        /// Alot of standard umbraco fields shouldn't be tokenized or even indexed, just stored into lucene
        /// for retreival after searching.
        /// </summary>
        public static readonly FieldDefinition[] UmbracoIndexFieldDefinitions =
        {
            new FieldDefinition("parentID", FieldDefinitionTypes.Integer),
            new FieldDefinition("level", FieldDefinitionTypes.Integer),
            new FieldDefinition("writerID", FieldDefinitionTypes.Integer),
            new FieldDefinition("creatorID", FieldDefinitionTypes.Integer),
            new FieldDefinition("sortOrder", FieldDefinitionTypes.Integer),
            new FieldDefinition("template", FieldDefinitionTypes.Integer),

            new FieldDefinition("createDate", FieldDefinitionTypes.DateTime),
            new FieldDefinition("updateDate", FieldDefinitionTypes.DateTime),

            new FieldDefinition(UmbracoExamineIndex.NodeKeyFieldName, FieldDefinitionTypes.InvariantCultureIgnoreCase),
            new FieldDefinition("version", FieldDefinitionTypes.Raw),
            new FieldDefinition("nodeType", FieldDefinitionTypes.InvariantCultureIgnoreCase),
            new FieldDefinition("template", FieldDefinitionTypes.Raw),
            new FieldDefinition("urlName", FieldDefinitionTypes.InvariantCultureIgnoreCase),
            new FieldDefinition("path", FieldDefinitionTypes.Raw),

            new FieldDefinition("email", FieldDefinitionTypes.EmailAddress),

            new FieldDefinition(UmbracoExamineIndex.PublishedFieldName, FieldDefinitionTypes.Raw),
            new FieldDefinition(UmbracoExamineIndex.IndexPathFieldName, FieldDefinitionTypes.Raw),
            new FieldDefinition(UmbracoExamineIndex.IconFieldName, FieldDefinitionTypes.Raw),
            new FieldDefinition(UmbracoContentIndex.VariesByCultureFieldName, FieldDefinitionTypes.Raw),
        };


        /// <summary>
        /// Overridden to dynamically add field definitions for culture variations
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldDefinition"></param>
        /// <returns></returns>
        /// <remarks>
        /// We need to do this so that we don't have to maintain a huge static list of all field names and their definitions
        /// otherwise we'd have to dynamically add/remove definitions anytime languages are added/removed, etc...
        /// For example, we have things like `nodeName` and `__Published` which are also used for culture fields like `nodeName_en-us`
        /// and we don't want to have a full static list of all of these definitions when we can just define the one definition and then
        /// dynamically apply that to culture specific fields.
        ///
        /// There is a caveat to this however, when a field definition is found for a non-culture field we will create and store a new field
        /// definition for that culture so that the next time it needs to be looked up and used we are not allocating more objects. This does mean
        /// however that if a language is deleted, the field definitions for that language will still exist in memory. This isn't going to cause any
        /// problems and the mem will be cleared on next site restart but it's worth pointing out.
        /// </remarks>
        public override bool TryGetValue(string fieldName, out FieldDefinition fieldDefinition)
        {
            if (base.TryGetValue(fieldName, out fieldDefinition))
                return true;

            //before we use regex to match do some faster simple matching since this is going to execute quite a lot
            if (!fieldName.Contains("_"))
                return false;

            var match = ExamineExtensions.CultureIsoCodeFieldNameMatchExpression.Match(fieldName);
            if (match.Success)
            {
                var nonCultureFieldName = match.Groups["FieldName"].Value;
                //check if there's a definition for this and if so return the field definition for the culture field based on the non-culture field
                if (base.TryGetValue(nonCultureFieldName, out var existingFieldDefinition))
                {
                    //now add a new field def
                    fieldDefinition = GetOrAdd(fieldName, s => new FieldDefinition(s, existingFieldDefinition.Type));
                    return true;
                }
            }
            return false;
        }

        
    }
}
