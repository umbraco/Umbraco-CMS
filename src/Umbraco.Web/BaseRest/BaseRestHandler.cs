using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Reflection;
using System.Xml;
using System.IO;

namespace Umbraco.Web.BaseRest
{
	internal class BaseRestHandler : IHttpHandler, IRequiresSessionState
	{
		const int ParametersOffset = 2;
		static string _baseUrl;

		static BaseRestHandler()
		{
			_baseUrl = UriUtility.ToAbsolute(Umbraco.Core.IO.SystemDirectories.Base).ToLower();
			if (!_baseUrl.EndsWith("/"))
				_baseUrl += "/";
		}

		public bool IsReusable
		{
			get { return true; }
		}

		/// <summary>
		/// Returns a value indicating whether a specified Uri should be routed to the BaseRestHandler.
		/// </summary>
		/// <param name="uri">The specified Uri.</param>
		/// <returns>A value indicating whether the specified Uri should be routed to the BaseRestHandler.</returns>
		public static bool IsBaseRestRequest(Uri uri)
		{
			return Umbraco.Core.Configuration.UmbracoSettings.EnableBaseRestHandler
				&& uri.AbsolutePath.ToLowerInvariant().StartsWith(_baseUrl);
		}

		public void ProcessRequest(HttpContext context)
		{
			string url = context.Request.RawUrl;

			// sanitize and split the url
			url = url.Substring(_baseUrl.Length);
			if (url.ToLower().Contains(".aspx"))
				url = url.Substring(0, url.IndexOf(".aspx"));
			if (url.ToLower().Contains("?"))
				url = url.Substring(0, url.IndexOf("?"));
			var urlParts = url.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

			// always return xml content
			context.Response.ContentType = "text/xml";

			// ensure that we have a valid request ie /base/library/method/[parameter].aspx
			if (urlParts.Length < ParametersOffset)
			{
				context.Response.Write("<error>Invalid request, missing parts.</error>");
				context.Response.StatusCode = 400;
				context.Response.StatusDescription = "Bad Request";
				context.Response.End();
				return;
			}

			// fixme - what follows comes straight from baseHttpModule.cs

			string extensionAlias = urlParts[0];
			string methodName = urlParts[1];

			var myExtension = new global::umbraco.presentation.umbracobase.restExtension(extensionAlias, methodName);

			if (myExtension.isAllowed)
			{
				TrySetCulture();

				string response = invokeMethod(myExtension, urlParts);
				if (response.Length >= 7 && response.Substring(0, 7) == "<error>")
				{
					context.Response.StatusCode = 500;
					context.Response.StatusDescription = "Internal Server Error";
				}
				context.Response.Output.Write(response);
			}
			else
			{
				context.Response.StatusCode = 500;
				context.Response.StatusDescription = "Internal Server Error";
				context.Response.Output.Write("<error>Extension not found or permission denied.</error>");
			}

			context.Response.End();
		}

		#region from baseHttpModule.cs

		// fixme - these methods have *not* been cleaned up, just partially fixed...

		string invokeMethod(global::umbraco.presentation.umbracobase.restExtension myExtension, string[] urlParts)
		{
			try
			{
				//So method is either not found or not valid... this should probably be moved... 
				if (!myExtension.method.IsPublic || !myExtension.method.IsStatic)
					return "<error>Method has to be public and static</error>";
				else //And if it is lets continue trying to invoke it... 
				{
					//lets check if we have parameters enough in the url..
					if (myExtension.method.GetParameters().Length > (urlParts.Length - ParametersOffset)) //Too few
						return "<error>Not Enough parameters in url</error>";
					else
					{

						//We have enough parameters... lets invoke..
						//Create an instance of the type we need to invoke the method from.
						//**Object obj = Activator.CreateInstance(myExtension.type);
						Object response;

						//umbracoBase.baseBinder bBinder = new baseBinder();

						if (myExtension.method.GetParameters().Length == 0)
						{
							//response = myMethod.method.Invoke(obj, BindingFlags.Public | BindingFlags.Instance, bBinder, null, System.Globalization.CultureInfo.CurrentCulture);
							response = myExtension.method.Invoke(null /*obj*/, null); //Invoke with null as parameters as there are none
						}

						else
						{
							//We only need the parts of the url above the number 4 so we'll 
							//recast those to objects and add them to the object[]

							//Getting the right lenght.. 4 is the magic number dum di dum.. 
							object[] methodParams = new object[urlParts.Length - ParametersOffset];

							int i = 0;

							foreach (ParameterInfo pInfo in myExtension.method.GetParameters())
							{
								Type myType = Type.GetType(pInfo.ParameterType.ToString());
								methodParams[(i)] = Convert.ChangeType(urlParts[i + ParametersOffset], myType);
								i++;
							}

							//Invoke with methodParams
							//response = myMethod.method.Invoke(obj, BindingFlags.Public | BindingFlags.Instance, bBinder, methodParams, System.Globalization.CultureInfo.CurrentCulture);
							response = myExtension.method.Invoke(/*obj*/null, methodParams);
						}

						/*TODO - SOMETHING ALITTLE BETTER THEN ONLY CHECK FOR XPATHNODEITERATOR OR ELSE do ToString() */
						if (response != null)
						{
							switch (myExtension.method.ReturnType.ToString())
							{
								case "System.Xml.XPath.XPathNodeIterator":
									return ((System.Xml.XPath.XPathNodeIterator)response).Current.OuterXml;
								case "System.Xml.Linq.XDocument":
									return response.ToString();
								case "System.Xml.XmlDocument":
									XmlDocument xmlDoc = (XmlDocument)response;
									StringWriter sw = new StringWriter();
									XmlTextWriter xw = new XmlTextWriter(sw);
									xmlDoc.WriteTo(xw);
									return sw.ToString();
								default:
									string strResponse = ((string)response.ToString());

									if (myExtension.returnXML)
									{
										//do a quick "is this html?" check... if it is add CDATA... 
										if (strResponse.Contains("<") || strResponse.Contains(">"))
											strResponse = "<![CDATA[" + strResponse + "]]>";
										return "<value>" + strResponse + "</value>";
									}
									else
									{
										HttpContext.Current.Response.ContentType = "text/html";
										return strResponse;
									}
							}
						}
						else
						{
							if (myExtension.returnXML)
								return "<error>Null value returned</error>";
							else
								return string.Empty;
						}



					}
				}
			}

			catch (Exception ex)
			{
				//Overall exception handling... 
				return "<error><![CDATA[MESSAGE:\n" + ex.Message + "\n\nSTACKTRACE:\n" + ex.StackTrace + "\n\nINNEREXCEPTION:\n" + ex.InnerException + "]]></error>";
			}
		}

		private static void TrySetCulture()
		{
			string domain = HttpContext.Current.Request.Url.Host; //Host only
			if (TrySetCulture(domain)) return;

			domain = HttpContext.Current.Request.Url.Authority; //Host with port
			if (TrySetCulture(domain)) return;
		}

		private static bool TrySetCulture(string domain)
		{
			var uDomain = global::umbraco.cms.businesslogic.web.Domain.GetDomain(domain);
			if (uDomain == null) return false;
			System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(uDomain.Language.CultureAlias);
			return true;
		}

		#endregion
	}
}
