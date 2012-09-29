using System.Text.RegularExpressions;
using umbraco;

namespace Umbraco.Web.Templates
{
	//NOTE: I realize there is only one class in this namespace but I'm pretty positive that there will be more classes in 
	//this namespace once we start migrating and cleaning up more code.

	/// <summary>
	/// Utility class used for templates
	/// </summary>
	public static class TemplateUtilities
	{
		/// <summary>
		/// Parses the string looking for the {localLink} syntax and updates them to their correct links.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static string ParseInternalLinks(string text)
		{
			var niceUrlsProvider = UmbracoContext.Current.NiceUrlProvider;

			// Parse internal links
			MatchCollection tags = Regex.Matches(text, @"href=""[/]?(?:\{|\%7B)localLink:([0-9]+)(?:\}|\%7D)", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
			foreach (Match tag in tags)
				if (tag.Groups.Count > 0)
				{
					string id = tag.Groups[1].Value; //.Remove(tag.Groups[1].Value.Length - 1, 1);
					string newLink = niceUrlsProvider.GetNiceUrl(int.Parse(id));
					text = text.Replace(tag.Value.ToString(), "href=\"" + newLink);
				}
			return text;
		}


	}
}
