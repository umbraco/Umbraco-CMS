namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides a method to try to find an assign an Umbraco document to a <c>DocumentRequest</c>.
	/// </summary>
    internal interface IRequestDocumentResolver
    {
		/// <summary>
		/// Tries to find and assign an Umbraco document to a <c>DocumentRequest</c>.
		/// </summary>
		/// <param name="docRequest">The <c>DocumentRequest</c> that is being resolved.</param>
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		/// <remarks>Optionally, can also assign the template, although that is not required.</remarks>
		bool TrySetDocument(DocumentRequest docRequest);
    }
}