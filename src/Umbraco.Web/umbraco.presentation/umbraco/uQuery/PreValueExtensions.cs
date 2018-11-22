using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.datatype;

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
            using(var sqlHelper = Application.SqlHelper)
			using (var reader = sqlHelper.ExecuteReader(
				"SELECT alias FROM cmsDataTypePreValues WHERE id = @id", 
				sqlHelper.CreateParameter("@id", preValue.Id)))
			{
				var hasRows = reader.Read();

				if (!hasRows)
				{
					return null;
				}

				var alias = string.Empty;

				while (hasRows)
				{
					if (reader.ContainsField("alias") && !reader.IsNull("alias"))
					{
						alias = reader.GetString("alias");
					}

					hasRows = reader.Read();
				}

				return alias;
			}
		}
	}
}