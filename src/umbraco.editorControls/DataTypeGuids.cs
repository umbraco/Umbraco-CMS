using System;
using Umbraco.Core;

namespace umbraco.editorControls
{
	/// <summary>
	/// Defines the Guids for all the DataTypes; e.g. quick access to 3rd-party developers.
	/// </summary>
	[Obsolete("This class is now obsolete please use Umbraco.Core.Constants.PropertyEditors", false)]
	public static class DataTypeGuids
	{
		/// <summary>
		/// Guid for the MultiNodeTreePicker data-type.
		/// </summary>
		[Obsolete("This property is now obsolete please use Umbraco.Core.Constants.PropertyEditors.MultiNodeTreePicker", false)]
		public const string MultiNodeTreePickerId = Constants.PropertyEditors.MultiNodeTreePicker;

		/// <summary>
		/// Guid for the MultipleTextstring data-type.
		/// </summary>
		[Obsolete("This property is now obsolete please use Umbraco.Core.Constants.PropertyEditors.MultipleTextstring", false)]
		public const string MultipleTextstringId = Constants.PropertyEditors.MultipleTextstring;

		/// <summary>
		/// Guid for the PickerRelations (previoulsy uComponents: MultiPickerRelations)
		/// </summary>
		[Obsolete("This property is now obsolete please use Umbraco.Core.Constants.PropertyEditors.PickerRelations", false)]
		public const string PickerRelationsId = Constants.PropertyEditors.PickerRelations;

		/// <summary>
		/// Guid for the Slider data-type.
		/// </summary>
		[Obsolete("This property is now obsolete please use Umbraco.Core.Constants.PropertyEditors.Slider", false)]
		public const string SliderId = Constants.PropertyEditors.Slider;

		/// <summary>
		/// Guid for the XPathCheckBoxList data-type.
		/// </summary>
		[Obsolete("This property is now obsolete please use Umbraco.Core.Constants.PropertyEditors.XPathCheckBoxList", false)]
		public const string XPathCheckBoxListId = Constants.PropertyEditors.XPathCheckBoxList;

		/// <summary>
		/// Guid for the XPathDropDownList data-type.
		/// </summary>
		[Obsolete("This property is now obsolete please use Umbraco.Core.Constants.PropertyEditors.XPathDropDownList", false)]
		public const string XPathDropDownListId = Constants.PropertyEditors.XPathDropDownList;
	}
}