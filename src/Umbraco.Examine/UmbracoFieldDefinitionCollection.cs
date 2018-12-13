using Examine;

namespace Umbraco.Examine
{
    /// <summary>
    /// Custom <see cref="FieldDefinitionCollection"/> allowing dynamic creation of <see cref="FieldDefinition"/>
    /// </summary>
    public class UmbracoFieldDefinitionCollection : FieldDefinitionCollection
    {
        
        public UmbracoFieldDefinitionCollection()
            : base(UmbracoExamineIndex.UmbracoIndexFieldDefinitions)
        {
        }

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