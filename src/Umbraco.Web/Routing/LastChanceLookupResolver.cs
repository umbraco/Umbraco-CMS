using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.Routing
{

	/// <summary>
	/// A singly registered object resolver to manage the IDocumentLastChanceLookup
	/// </summary>
	internal sealed class LastChanceLookupResolver : SingleObjectResolverBase<LastChanceLookupResolver, IDocumentLastChanceLookup>
	{
	
		internal LastChanceLookupResolver(IDocumentLastChanceLookup lastChanceLookup)
			: base(lastChanceLookup)
		{
		} 
	
		/// <summary>
		/// Can be used by developers at runtime to set their DocumentLastChanceLookup at app startup
		/// </summary>
		/// <param name="lastChanceLookup"></param>
		public void SetLastChanceLookup(IDocumentLastChanceLookup lastChanceLookup)
		{
			Value = lastChanceLookup;
		}

		/// <summary>
		/// Returns the Last Chance Lookup
		/// </summary>
		public IDocumentLastChanceLookup LastChanceLookup
		{
			get { return Value; }
		}

	}
}