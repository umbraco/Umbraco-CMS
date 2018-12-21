using Examine;

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

            new FieldDefinition("key", FieldDefinitionTypes.InvariantCultureIgnoreCase),
            new FieldDefinition("version", FieldDefinitionTypes.Raw),
            new FieldDefinition("nodeType", FieldDefinitionTypes.InvariantCultureIgnoreCase),
            new FieldDefinition("template", FieldDefinitionTypes.Raw),
            new FieldDefinition("urlName", FieldDefinitionTypes.InvariantCultureIgnoreCase),
            new FieldDefinition("path", FieldDefinitionTypes.Raw),

            new FieldDefinition("email", FieldDefinitionTypes.EmailAddress),

            new FieldDefinition(UmbracoExamineIndex.PublishedFieldName, FieldDefinitionTypes.Raw),
            new FieldDefinition(UmbracoExamineIndex.NodeKeyFieldName, FieldDefinitionTypes.Raw),
            new FieldDefinition(UmbracoExamineIndex.IndexPathFieldName, FieldDefinitionTypes.Raw),
            new FieldDefinition(UmbracoExamineIndex.IconFieldName, FieldDefinitionTypes.Raw)
        };

        ///// <summary>
        ///// Overridden to dynamically add field definitions for culture variations
        ///// </summary>
        ///// <param name="fieldName"></param>
        ///// <param name="fieldDefinition"></param>
        ///// <returns></returns>
        //public override bool TryGetValue(string fieldName, out FieldDefinition fieldDefinition)
        //{
        //    var result = base.TryGetValue(fieldName, out fieldDefinition);
        //    if (result) return true;

        //    //if the fieldName is not suffixed with _iso-Code
        //    var underscoreIndex = fieldName.LastIndexOf('_');
        //    if (underscoreIndex == -1) return false;



        //    var isoCode = fieldName.Substring(underscoreIndex);
        //    if (isoCode.Length < 6) return false; //invalid isoCode

        //    var hyphenIndex = isoCode.IndexOf('-');
        //    if (hyphenIndex != 3) return false; //invalid isoCode

        //    //we'll assume this is a valid isoCode

        //}
    }
}
