using System;

namespace umbraco.interfaces
{
	/// <summary>
	/// The IDataType is a interface used for adding a new Data type to the umbraco backoffice.
    /// It consists of IdataEditor which provides the Editing UI, the IDataPrevalue which provides prevalues and th their editing UI
    /// And finally it contains IData which manages the actual data in the Data Type
	/// </summary>
	[Obsolete("IDataType is obsolete and is no longer used, it will be removed from the codebase in future versions")]
	public interface IDataType 
	{
        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
		Guid Id {get;}
        /// <summary>
        /// Gets the name of the data type.
        /// </summary>
        /// <value>The name of the data type.</value>
		string DataTypeName{get;}
        /// <summary>
        /// Gets the data editor.
        /// </summary>
        /// <value>The data editor.</value>
		IDataEditor DataEditor{get;}
        /// <summary>
        /// Gets the prevalue editor.
        /// </summary>
        /// <value>The prevalue editor.</value>
		IDataPrevalue PrevalueEditor {get;}
        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <value>The data.</value>
		IData Data{get;}
        /// <summary>
        /// Gets or sets the data type definition id.
        /// </summary>
        /// <value>The data type definition id.</value>
		int DataTypeDefinitionId {set; get;}
	}

}
