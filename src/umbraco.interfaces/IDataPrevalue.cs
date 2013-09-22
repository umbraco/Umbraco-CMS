using System;

namespace umbraco.interfaces
{
    /// <summary>
    /// The IDataPrevalue interface is part of the IDataType interface for creating new data types in the umbraco backoffice. 
    /// The IDataPrevalue represents the editing UI for adding prevalues to the datatype.
    /// </summary>
    [Obsolete("IDataPrevalue is obsolete and is no longer used, it will be removed from the codebase in future versions")]
	public interface IDataPrevalue 
	{
        /// <summary>
        /// Saves this instance.
        /// </summary>
		void Save();
        /// <summary>
        /// Gets the editor control.
        /// </summary>
        /// <value>The editor.</value>
		System.Web.UI.Control Editor{get;}
	}
}
