using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace umbraco.presentation.ClientDependency
{
	public class CssFileUrlFormatter
	{

		/// <summary>
		/// Returns the CSS file with all of the url's formatted to be absolute locations
		/// </summary>
		/// <param name="fileContent">content of the css file</param>
		/// <param name="cssLocation">the uri location of the css file</param>
		/// <returns></returns>
		public static string TransformCssFile(string fileContent, Uri cssLocation)
		{
			string str = Regex.Replace(
				fileContent,
				@"url\((.+)\)",
				new MatchEvaluator(
					delegate(Match m)
					{
						if (m.Groups.Count == 2)
						{
							string match = m.Groups[1].Value.Trim('\'', '"');
							return string.Format(@"url(""{0}"")", 
								match.StartsWith("http") ? match : new Uri(cssLocation, match).ToString());
						}
						return m.Value;
					})
				);

			return str;
		}

	}
}
