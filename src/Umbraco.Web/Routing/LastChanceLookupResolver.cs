using Umbraco.Core.Resolving;

namespace Umbraco.Web.Routing
{

	/// <summary>
	/// A singly registered object resolver to manage the IDocumentLastChanceLookup
	/// </summary>
	internal sealed class LastChanceLookupResolver : SingleObjectResolverBase<IDocumentLastChanceLookup>
	{
		#region Singleton
		private static readonly LastChanceLookupResolver Instance = new LastChanceLookupResolver(new DefaultLastChanceLookup());
		public static LastChanceLookupResolver Current
		{
			get { return Instance; }
		} 
		#endregion

		#region Constructors
		static LastChanceLookupResolver() { }
		internal LastChanceLookupResolver(IDocumentLastChanceLookup lastChanceLookup)
		{
			Value = lastChanceLookup;
		} 
		#endregion

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