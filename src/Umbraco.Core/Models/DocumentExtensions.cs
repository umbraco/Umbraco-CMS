using System.Linq;

namespace Umbraco.Core.Models
{
	/// <summary>
	/// Extension methods for IDocument
	/// </summary>
	public static class DocumentExtensions
	{
		/// <summary>
		/// Returns the property based on the case insensitive match of the alias
		/// </summary>
		/// <param name="d"></param>
		/// <param name="alias"></param>
		/// <returns></returns>
		public static IDocumentProperty GetProperty(this IDocument d, string alias)
		{
			return d.Properties.FirstOrDefault(p => p.Alias.InvariantEquals(alias));
		}
	}
}