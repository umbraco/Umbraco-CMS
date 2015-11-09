using System;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Maps a property source value to a data object.
    /// </summary>
    // todo: drop IPropertyEditorValueConverter support (when?).
    [Obsolete("Use IPropertyValueConverter.")]
    public interface IPropertyEditorValueConverter
	{
		/// <summary>
		/// Returns a value indicating whether this provider applies to the specified property.
		/// </summary>
		/// <param name="datatypeGuid">A Guid identifying the property datatype.</param>
		/// <param name="contentTypeAlias">The content type alias.</param>
		/// <param name="propertyTypeAlias">The property alias.</param>
		/// <returns>True if this provider applies to the specified property.</returns>
		bool IsConverterFor(Guid datatypeGuid, string contentTypeAlias, string propertyTypeAlias);

		/// <summary>
		/// Attempts to convert a source value specified into a property model.
		/// </summary>
		/// <param name="sourceValue">The source value.</param>
		/// <returns>An <c>Attempt</c> representing the result of the conversion.</returns>
		/// <remarks>The source value is dependent on the content cache. With the Xml content cache it
		/// is always a string, but with other caches it may be an object (numeric, time...) matching
		/// what is in the database. Be prepared.</remarks>
		Attempt<object> ConvertPropertyValue(object sourceValue);
	}
}