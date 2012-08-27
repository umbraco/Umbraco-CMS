using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Core
{
	//NOTE: I'm not sure what this does or what it is used for, have emailed Gareth about it as the name really means nothing to me 
	// and don't know where this class actually belongs.

    internal class RazorDataTypeModelStaticMappingItem
    {
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
