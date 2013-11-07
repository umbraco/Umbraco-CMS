using System.Linq;
using umbraco.cms.businesslogic.datatype;
using Umbraco.Core;

namespace umbraco
{
	/// <summary>
	/// uQuery extensions for the PreValue object.
	/// </summary>
	public static class PreValueExtensions
	{
		/// <summary>
		/// Gets the alias of the specified PreValue
		/// </summary>
		/// <param name="preValue">The PreValue.</param>
		/// <returns>The alias</returns>
		public static string GetAlias(this PreValue preValue)
		{
            return ApplicationContext.Current.DatabaseContext.Database.SingleOrDefault<string>(
                "SELECT alias FROM cmsDataTypePreValues WHERE id = @id", new { id = preValue.Id });
		}
	}
}