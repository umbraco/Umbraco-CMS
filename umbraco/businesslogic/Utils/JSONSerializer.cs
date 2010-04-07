using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;

namespace umbraco.businesslogic.Utils
{
	/// <summary>
	/// The built in JavaScriptSerializer does not allow you to export real JavaScript
	/// objects, functions, etc... only string values which isn't always what you want.
	/// See <see cref="example"/>
	/// 
	/// Override the JavaScriptSerializer serialization process and look for any
	/// custom "tags" strings such as a @ symbol which depicts that the string value 
	/// should really be a JSON value, therefore the output removes the double quotes.
	/// 
	/// </summary>
	/// <example>
	/// If you want to output:
	/// {"myFunction": function() {alert('hello');}}
	/// The JavaScriptSerializer will not let you do this, it will render:
	/// {"myFunction": "function() {alert('hello');}"}
	/// which means that JavaScript will interpret it as a string.
	/// This class allows you to output JavaScript objects, amongst other things.
	/// </example>
	public class JSONSerializer : JavaScriptSerializer
	{

		public new string Serialize(object obj)
		{
			string output = base.Serialize(obj);

			//replaces all strings beginning with this prefix to have no double quotes
			Regex regex1 = new Regex(string.Format("(\"{0}(.*?)\")+", PrefixJavaScriptObject),
				RegexOptions.Multiline
				| RegexOptions.CultureInvariant
				| RegexOptions.Compiled
			);
			string result = regex1.Replace(output, "$2");
		
			return result;
		}

		private const string PrefixJavaScriptObject = "@@@@";

		/// <summary>
		/// method for a string to be converted to a json object.
		/// </summary>
		/// <param name="s"></param>
		/// <returns>A string formatted with a special prefix</returns>
		/// <remarks>
		/// This essentially just prefixes the string with a special key that we will use
		/// to parse with later during serialization.
		/// </remarks>
		public static string ToJSONObject(string s)
		{
			return PrefixJavaScriptObject + s;
		}

	
	}

	

}

