using System;

namespace umbraco.editorControls.MultiNodeTreePicker
{
	/// <summary>
	/// An enumerator for the XPath filter, for either enable/disable.
	/// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
	public enum XPathFilterType
	{
		/// <summary>
		/// Disables the XPath filter.
		/// </summary>
		Disable,

		/// <summary>
		/// Enables the XPath filter.
		/// </summary>
		Enable
	}
}