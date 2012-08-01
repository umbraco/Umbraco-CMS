using System;
using System.Xml.XPath; 
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;

namespace umbraco.presentation.xslt.Exslt
{
	/// <summary>
	/// Implements the functions in the http://exslt.org/strings namespace 
	/// </summary>
	public class ExsltStrings {
		/// <summary>
		/// Implements the following function 
		///		node-set tokenize(string, string)
		/// </summary>
		/// <param name="str"></param>
		/// <param name="delimiters"></param>		
		/// <param name="flags"></param>
		/// <returns>This function breaks the input string into a sequence of strings, 
		/// treating any character in the list of delimiters as a separator. 
		/// The separators themselves are not returned. 
		/// The tokens are returned as a set of 'token' elements.</returns>
		public static XPathNodeIterator tokenize(string str, string delimiters){					

			XmlDocument doc = new XmlDocument(); 
			doc.LoadXml("<tokens/>"); 
			 
			foreach(string token in str.Split(delimiters.ToCharArray())){
			
				XmlElement elem = doc.CreateElement("token"); 
				elem.InnerText  = token; 
				doc.DocumentElement.AppendChild(elem); 
			}

			return doc.CreateNavigator().Select("//token"); 
		}


		/// <summary>
		/// Implements the following function 
		///		node-set tokenize(string)
		/// </summary>
		/// <param name="str"></param>
		/// <param name="delimiters"></param>		
		/// <param name="flags"></param>
		/// <returns>This function breaks the input string into a sequence of strings, 
		/// using the whitespace characters as a delimiter. 
		/// The separators themselves are not returned. 
		/// The tokens are returned as a set of 'token' elements.</returns>
		public static XPathNodeIterator tokenize(string str){					

			Regex regex = new Regex("\\s+"); 

			XmlDocument doc = new XmlDocument(); 
			doc.LoadXml("<tokens/>"); 			
			 
			foreach(string token in regex.Split(str)){
			
				XmlElement elem = doc.CreateElement("token"); 
				elem.InnerText  = token; 
				doc.DocumentElement.AppendChild(elem); 
			}

			return doc.CreateNavigator().Select("//token"); 
		}

/// <summary>
/// Implements the following function 
///		string replace(string, string, string) 
/// </summary>
/// <param name="str"></param>
/// <param name="oldValue"></param>
/// <param name="newValue"></param>
/// <returns></returns>
/// <remarks>This function has completely diffeerent semantics from the EXSLT function. 
/// The description of the EXSLT function is confusing and furthermore no one has implemented
/// the described semantics which implies that others find the method problematic. Instead
/// this function is straightforward, it replaces all occurrences of oldValue with 
/// newValue</remarks>
		public static string replace(string str, string oldValue, string newValue){

			return str.Replace(oldValue, newValue); 
		}

/// <summary>
/// Implements the following function 
///		string padding(number)
/// </summary>
/// <param name="number"></param>
/// <returns></returns>
		public static string padding(int number){
		
			
			string s = String.Empty; 

			if(number < 0){
				return s;
			}else{
				return s.PadLeft(number); 
			}
		}

		/// <summary>
		/// Implements the following function 
		///		string padding(number, string)
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		public static string padding(int number, string s){								 

			if(number < 0){
				return String.Empty;
			}else{
				StringBuilder sb = new StringBuilder(s); 
				
				while(sb.Length < number){ 				
					sb.Append(s); 
				}

				if(sb.Length > number){
					return sb.Remove(number, sb.Length - number).ToString(); 	
				}else{
					return sb.ToString(); 
				}
			}
		}


		/// <summary>
		/// Implements the following function 
		///		string uppercase(string)
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		/// <remarks>THIS FUNCTION IS NOT IN EXSLT!!!</remarks>
		public static string uppercase(string str){
			return str.ToUpper(); 
		}

		/// <summary>
		/// Implements the following function 
		///		string lowercase(string)
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		/// <remarks>THIS FUNCTION IS NOT IN EXSLT!!!</remarks>
		public static string lowercase(string str){
			return str.ToLower(); 
		}

		/// <summary>
		/// Implements the following function 
		///		node-set split(string)
		/// </summary>
		/// <param name="str"></param>
		/// <remarks>This function breaks the input string into a sequence of strings, 
		/// using the space character as a delimiter. 
		/// The space character itself is never returned not even when there are 
		/// adjacent space characters. 
		/// </remarks>
		/// <returns>The tokens are returned as a set of 'token' elements</returns>
		public static XPathNodeIterator split(string str){
		
			XmlDocument doc = new XmlDocument(); 
			doc.LoadXml("<tokens/>"); 
			

			foreach(string match in str.Split(new char[]{' '})){
			
				if(!match.Equals(String.Empty)){
					XmlElement elem = doc.CreateElement("token"); 
					elem.InnerText  = match; 
					doc.DocumentElement.AppendChild(elem); 
				}
			}

			return doc.CreateNavigator().Select("//token"); 

		}

		/// <summary>
		/// Implements the following function 
		///		node-set split(string, string)
		/// </summary>
		/// <param name="str"></param>
		/// <param name="delimiter"></param>
		/// <remarks>This function breaks the input string into a sequence of strings, 
		/// using the space character as a delimiter. 
		/// The space character itself is never returned not even when there are 
		/// adjacent space characters. 
		/// </remarks>
		/// <returns>The tokens are returned as a set of 'token' elements</returns>
		public static XPathNodeIterator split(string str, string delimiter){
		
			XmlDocument doc = new XmlDocument(); 
			doc.LoadXml("<tokens/>"); 
			

			if(delimiter.Equals(String.Empty)){
				foreach(char match in str){
			
					XmlElement elem = doc.CreateElement("token"); 
					elem.InnerText  = match.ToString(); 
					doc.DocumentElement.AppendChild(elem); 
				}
			}else{
				//since there is no String.Split(string) method we use the Regex class 
				//and escape special characters. 
				//. $ ^ { [ ( | ) * + ? \
				delimiter = delimiter.Replace("\\","\\\\").Replace("$", "\\$").Replace("^", "\\^");
				delimiter = delimiter.Replace("{", "\\{").Replace("[", "\\[").Replace("(", "\\("); 
				delimiter = delimiter.Replace("*","\\*").Replace(")", "\\)").Replace("|", "\\|");
				delimiter = delimiter.Replace("+", @"\+").Replace("?", "\\?").Replace(".", "\\."); 
				 				
				Regex regex = new Regex(delimiter); 


				foreach(string match in regex.Split(str)){
			
					if((!match.Equals(String.Empty)) && (!match.Equals(delimiter))){
						XmlElement elem = doc.CreateElement("token"); 
						elem.InnerText  = match; 
						doc.DocumentElement.AppendChild(elem); 
					}
				}
			}

			return doc.CreateNavigator().Select("//token"); 
		}

	/// <summary>
	/// Implements the following function 
	///		string concat(node-set)
	/// </summary>
	/// <param name="nodeset"></param>
	/// <returns></returns>
		public static string concat(XPathNodeIterator nodeset){

			StringBuilder sb = new StringBuilder(); 
		
			while(nodeset.MoveNext()){
				sb.Append(nodeset.Current.Value); 
			}

			return sb.ToString(); 
		}
	}
}

