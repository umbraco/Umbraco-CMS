namespace Umbraco.Core.ObjectResolution
{
	/// <summary>
	/// Specifies the lifetime scope of resolved objects.
	/// </summary>
	public enum ObjectLifetimeScope
	{
		/// <summary>
		/// A per-request object instance is created.
		/// </summary>
		HttpRequest,

		/// <summary>
		/// A single application-wide object instance is created.
		/// </summary>
		Application,

		/// <summary>
		/// A new object instance is created each time one is requested.
		/// </summary>
		Transient
	}
}