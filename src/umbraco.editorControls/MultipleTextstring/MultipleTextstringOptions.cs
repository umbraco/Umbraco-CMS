using System;
using System.ComponentModel;
using umbraco.cms.businesslogic.datatype;

namespace umbraco.editorControls.MultipleTextstring
{
	/// <summary>
	/// The options for the Multiple Textstring data-type.
	/// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
	public class MultipleTextstringOptions : AbstractOptions
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MultipleTextstringOptions"/> class.
		/// </summary>
		public MultipleTextstringOptions()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MultipleTextstringOptions"/> class.
		/// </summary>
		/// <param name="loadDefaults">if set to <c>true</c> [load defaults].</param>
		public MultipleTextstringOptions(bool loadDefaults)
			: base(loadDefaults)
		{
		}

		/// <summary>
		/// Gets or sets the maximum.
		/// </summary>
		/// <value>The maximum.</value>
		[DefaultValue(-1)]
		public int Maximum { get; set; }

		/// <summary>
		/// Gets or sets the minimum.
		/// </summary>
		/// <value>The minimum.</value>
		[DefaultValue(1)]
		public int Minimum { get; set; }
	}
}
