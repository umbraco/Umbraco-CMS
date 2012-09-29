using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Umbraco.Core.Templates
{
	internal class TemplateUtilities
	{

		public static string ParseInternalLinks(string pageContents)
		{
			// Parse internal links
			MatchCollection tags = Regex.Matches(pageContents, @"href=""[/]?(?:\{|\%7B)localLink:([0-9]+)(?:\}|\%7D)", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
			foreach (Match tag in tags)
				if (tag.Groups.Count > 0)
				{
					string id = tag.Groups[1].Value; //.Remove(tag.Groups[1].Value.Length - 1, 1);
					string newLink = library.NiceUrl(int.Parse(id));
					pageContents = pageContents.Replace(tag.Value.ToString(), "href=\"" + newLink);
				}
			return pageContents;
		}


	}
}
