namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides a method to try to find an assign an Umbraco document to a <c>PublishedContentRequest</c> when everything else has failed.
	/// </summary>
	internal interface IDocumentLastChanceLookup
    {
		/// <summary>
		/// Tries to find and assign an Umbraco document to a <c>PublishedContentRequest</c>.
		/// </summary>
		/// <param name="docRequest">The <c>PublishedContentRequest</c>.</param>
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		/// <remarks>Optionally, can also assign the template, although that is not required.</remarks>
		bool TrySetDocument(PublishedContentRequest docRequest);
    }
}