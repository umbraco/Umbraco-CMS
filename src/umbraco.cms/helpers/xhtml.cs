using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Umbraco.Core.Configuration;


namespace umbraco.cms.helpers
{
	/// <summary>
	/// Summary description for xhtml.
	/// </summary>
	public class xhtml
	{
		public xhtml()
		{
			//
			// TODO: Add constructor logic here
			//
		}

        public static string TidyHtml(string html) {
            TidyNet.Tidy tidy = new TidyNet.Tidy();

            /* Set the options you want */
            tidy.Options.DocType = TidyNet.DocType.Strict;
            tidy.Options.DropFontTags = true;
            tidy.Options.LogicalEmphasis = true;
            if (GlobalSettings.EditXhtmlMode == "true")
            {
                tidy.Options.Xhtml = true;
                tidy.Options.XmlOut = true;
            }
            else {
                tidy.Options.XmlOut = false;
                tidy.Options.Xhtml = false;
            }
            tidy.Options.MakeClean = true;
            tidy.Options.TidyMark = false;

            // To avoid entity encoding
            tidy.Options.CharEncoding = (TidyNet.CharEncoding)Enum.Parse(typeof(TidyNet.CharEncoding), UmbracoConfig.For.UmbracoSettings().Content.TidyCharEncoding);


            /* Declare the parameters that is needed */
            TidyNet.TidyMessageCollection tmc = new TidyNet.TidyMessageCollection();
            MemoryStream input = new MemoryStream();
            MemoryStream output = new MemoryStream();

            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(html);
            input.Write(byteArray, 0, byteArray.Length);
            input.Position = 0;
            tidy.Parse(input, output, tmc);

            string tidyed = System.Text.Encoding.UTF8.GetString(output.ToArray());

            // only return body
            string regex = @"</{0,1}body[^>]*>";
            System.Text.RegularExpressions.RegexOptions options = ((System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace | System.Text.RegularExpressions.RegexOptions.Multiline)
                | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(regex, options);
            string[] s = reg.Split(tidyed);
            if (s.Length > 1)
                return s[1];
            else
                return "[tidy error]";
        }

		public static string BootstrapTidy(string html) 
		{
			string emptyTags = ",br,hr,input,img,";	
			string regex = "(<[^\\?][^(>| )]*>)|<([^\\?][^(>| )]*)([^>]*)>";
			Hashtable replaceTag = new Hashtable();
			replaceTag.Add("strong", "b");
			replaceTag.Add("em", "i");

			System.Text.RegularExpressions.RegexOptions options = ((System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace | System.Text.RegularExpressions.RegexOptions.Multiline) 
				| System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(regex, options);

			foreach (Match m in reg.Matches(html)) 
			{
				string orgTag = "";
				string tag = "";
				string cleanTag = "";

				if (m.Groups.Count < 2 || (m.Groups[2].Value.ToLower() != "img" || (m.Groups[2].Value.ToLower() == "img" && m.Value.IndexOf ("?UMBRACO_MACRO") == -1))) 
				{

					if (m.Groups[1].Value != "") 
					{
						orgTag = m.Groups[1].Value;
						cleanTag = replaceTags(m.Groups[1].Value.ToLower().Replace("<", "").Replace("/>", "").Replace(">", "").Trim(), replaceTag);
						tag = "<" + cleanTag + ">";
					}
					else 
					{
						orgTag = "<" + m.Groups[2].Value + m.Groups[3].Value + ">";

						// loop through the attributes and make them lowercase
						cleanTag = replaceTags(m.Groups[2].Value.ToLower(), replaceTag);
						tag = "<" + cleanTag + returnLowerCaseAttributes(m.Groups[3].Value) + ">";
					}

					// Check for empty tags
					if (bool.Parse(GlobalSettings.EditXhtmlMode) && emptyTags.IndexOf(","+cleanTag+",") > -1 && tag.IndexOf("/>") == -1)
						tag = tag.Replace(">", " />");

					html = html.Replace(orgTag, tag);
				}

			}
			return html;
		}

		private static string replaceTags(string tag, Hashtable replaceTag) 
		{
			string closeBracket = "";
			if (tag.Substring(0,1) == "/") 
			{
				closeBracket = "/";
				tag = tag.Substring(1, tag.Length-1);
			}

			if (replaceTag.ContainsKey(tag))
				return closeBracket+replaceTag[tag].ToString();
			else
				return closeBracket+tag;
		}

		[Obsolete("Has been superceded by Umbraco.Core.XmlHelper.GetAttributesFromElement")]
		public static Hashtable ReturnAttributes(String tag)
		{
			var h = new Hashtable();
			foreach (var i in Umbraco.Core.XmlHelper.GetAttributesFromElement(tag))
			{
				h.Add(i.Key, i.Value);
			}
			return h;
		}

		private static string returnLowerCaseAttributes(String tag) 
		{
			string newTag = "";
			MatchCollection m = Regex.Matches(tag, "(?<attributeName>\\S*)=\"(?<attributeValue>[^\"]*)\"|(?<attributeName>\\S*)=(?<attributeValue>[^( |>)]*)(>| )",  RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
			foreach (System.Text.RegularExpressions.Match attributeSet in m) 
				newTag += " " + attributeSet.Groups["attributeName"].Value.ToString().ToLower() + "=\"" + attributeSet.Groups["attributeValue"].Value.ToString() + "\"";

			return newTag;
		}

        // helper method gotten from:
        // http://stackoverflow.com/questions/20762/how-do-you-remove-invalid-hexadecimal-characters-from-an-xml-based-data-source-p#comment8130028_641632
	    internal static string RemoveIllegalXmlCharacters(string inString)
        {

            if (inString == null) return null;

            StringBuilder sbOutput = new StringBuilder();
            char ch;

            for (int i = 0; i < inString.Length; i++)
            {
                ch = inString[i];
                if ((ch >= 0x0020 && ch <= 0xD7FF) ||
                    (ch >= 0xE000 && ch <= 0xFFFD) ||
                    ch == 0x0009 ||
                    ch == 0x000A ||
                    ch == 0x000D)
                {
                    sbOutput.Append(ch);
                }
            }
            return sbOutput.ToString();

        }
	}
}
