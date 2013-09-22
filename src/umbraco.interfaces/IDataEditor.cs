using System;

namespace umbraco.interfaces
{

    /// <summary>
    /// The IDataEditor is part of the IDataType interface for creating new data types in the umbraco backoffice. 
    /// The IDataEditor represents the editing UI for the Data Type.
    /// </summary>
    [Obsolete("IDataEditor is obsolete and is no longer used, it will be removed from the codebase in future versions")]
	public interface IDataEditor 
	{
        /// <summary>
        /// Saves this instance.
        /// </summary>
		void Save();
        /// <summary>
        /// Gets a value indicating whether a label is shown
        /// </summary>
        /// <value><c>true</c> if [show label]; otherwise, <c>false</c>.</value>
		bool ShowLabel {get;}
        /// <summary>
        /// Gets a value indicating whether the editor should be treated as a rich text editor. 
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [treat as rich text editor]; otherwise, <c>false</c>.
        /// </value>
		bool TreatAsRichTextEditor {get;}
        /// <summary>
        /// Gets the editor control
        /// </summary>
        /// <value>The editor.</value>
		System.Web.UI.Control Editor{get;}
	}
}
