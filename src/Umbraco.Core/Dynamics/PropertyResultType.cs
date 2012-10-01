namespace Umbraco.Core.Dynamics
{
	/// <summary>
	/// Currently just used for informational purposes as to where a PropertyResult object was created from.
	/// </summary>
	internal enum PropertyResultType
	{
		/// <summary>
		/// The property resolved was a normal document property
		/// </summary>
		UserProperty,

		/// <summary>
		/// The property resolved was a property defined as a member on the document object (IPublishedContent) itself
		/// </summary>
		ReflectedProperty,

		/// <summary>
		/// The property was created manually for a custom purpose
		/// </summary>
		CustomProperty
	}
}