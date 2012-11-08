using System;
using System.Collections.Generic;
using System.Text;

namespace Umbraco.Core.Macros
{
	/// <summary>
	/// Parses the macro syntax in a string and renders out it's contents
	/// </summary>
	internal class MacroTagParser
	{
		/// <summary>
		/// This will accept a text block and seach/parse it for macro markup.
		/// When either a text block or a a macro is found, it will call the callback method.
		/// </summary>
		/// <param name="text"> </param>
		/// <param name="textFoundCallback"></param>
		/// <param name="macroFoundCallback"></param>
		/// <returns></returns>
		/// <remarks>
		/// This method  simply parses the macro contents, it does not create a string or result, 
		/// this is up to the developer calling this method to implement this with the callbacks.
		/// </remarks>
		internal static void ParseMacros(
			string text,
			Action<string> textFoundCallback, 
			Action<string, Dictionary<string, string>> macroFoundCallback )
		{
			if (textFoundCallback == null) throw new ArgumentNullException("textFoundCallback");
			if (macroFoundCallback == null) throw new ArgumentNullException("macroFoundCallback");

			string elementText = text;

			var fieldResult = new StringBuilder(elementText);

			//NOTE: This is legacy code, this is definitely not the correct way to do a while loop! :)
			var stop = false;
			while (!stop)
			{
				var tagIndex = fieldResult.ToString().ToLower().IndexOf("<?umbraco");
				if (tagIndex < 0)
					tagIndex = fieldResult.ToString().ToLower().IndexOf("<umbraco:macro");
				if (tagIndex > -1)
				{
					var tempElementContent = "";

					//text block found, call the call back method
					textFoundCallback(fieldResult.ToString().Substring(0, tagIndex));

					fieldResult.Remove(0, tagIndex);

					var tag = fieldResult.ToString().Substring(0, fieldResult.ToString().IndexOf(">") + 1);
					var attributes = XmlHelper.GetAttributesFromElement(tag);

					// Check whether it's a single tag (<?.../>) or a tag with children (<?..>...</?...>)
					if (tag.Substring(tag.Length - 2, 1) != "/" && tag.IndexOf(" ") > -1)
					{
						String closingTag = "</" + (tag.Substring(1, tag.IndexOf(" ") - 1)) + ">";
						// Tag with children are only used when a macro is inserted by the umbraco-editor, in the
						// following format: "<?UMBRACO_MACRO ...><IMG SRC="..."..></?UMBRACO_MACRO>", so we
						// need to delete extra information inserted which is the image-tag and the closing
						// umbraco_macro tag
						if (fieldResult.ToString().IndexOf(closingTag) > -1)
						{
							fieldResult.Remove(0, fieldResult.ToString().IndexOf(closingTag));
						}
					}


					var macroAlias = attributes["macroalias"] ?? attributes["alias"];

					//call the callback now that we have the macro parsed
					macroFoundCallback(macroAlias, attributes);

					fieldResult.Remove(0, fieldResult.ToString().IndexOf(">") + 1);
					fieldResult.Insert(0, tempElementContent);
				}
				else
				{
					//text block found, call the call back method
					textFoundCallback(fieldResult.ToString());

					stop = true; //break;
				}
			}
		}
	}
}