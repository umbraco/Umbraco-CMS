using System;

namespace Umbraco.Core.Configuration
{

	//NOTE: This is used in the old DynamicNode for performing value conversions/mappings for certain data types.
	// it has been obsoleted because we've replaced this with the PropertyEditorValueConvertersResolver which can 
	// have converters registered in code so we don't have to rely on even more config sections. 
	// These things probably won't need to be created all that often and in code is much easier to do.

    internal class RazorDataTypeModelStaticMappingItem
    {
        [Obsolete("This is not used whatsoever")]
		public string Raw { get; set; }
        //if all of the set (non null) properties match the property data currently being evaluated
		public string PropertyTypeAlias { get; set; }
		public string NodeTypeAlias { get; set; }
		public Guid? DataTypeGuid { get; set; }
		public string TypeName { get; set; }

    	public bool Applies(Guid dataTypeGuid, string nodeTypeAlias, string propertyTypeAlias)
        {
            return
            (
                (this.NodeTypeAlias != null || this.PropertyTypeAlias != null || this.DataTypeGuid != null) &&
                ((this.DataTypeGuid != null && this.DataTypeGuid == dataTypeGuid) || this.DataTypeGuid == null) &&
                ((this.PropertyTypeAlias != null && this.PropertyTypeAlias == propertyTypeAlias) || this.PropertyTypeAlias == null) &&
                ((this.NodeTypeAlias != null && this.NodeTypeAlias == nodeTypeAlias) || this.NodeTypeAlias == null)
            );
        }
    }
}
