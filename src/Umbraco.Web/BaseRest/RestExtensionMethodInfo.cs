using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml;
using System.IO;

using Umbraco.Core;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.member;

namespace Umbraco.Web.BaseRest
{
	class RestExtensionMethodInfo
	{
		#region Utilities

		static char[] Split = new char[] { ',' };

		static string[] SplitString(string s)
		{
			if (string.IsNullOrWhiteSpace(s))
				return new string[] { };
			else
				return s.ToLower().Split(Split, StringSplitOptions.RemoveEmptyEntries);
		}

		static string GetAttribute(XmlNode node, string name)
		{
			var attribute = node.Attributes[name];
			return attribute == null ? null : attribute.Value;
		}

		#endregion

		private RestExtensionMethodInfo()
		{
			this.Exists = false;
		}

		private RestExtensionMethodInfo(bool allowAll, string allowGroup, string allowType, string allowMember, bool returnXml, MethodInfo method)
		{
			this.Exists = true;
			_allowAll = allowAll;
			_allowGroups = SplitString(allowGroup);
			_allowTypes = SplitString(allowType);
			_allowMembers = SplitString(allowMember);
			this.ReturnXml = returnXml;
			_method = method;
		}

		static RestExtensionMethodInfo MissingMethod = new RestExtensionMethodInfo();
		static Dictionary<string, RestExtensionMethodInfo> _cache = new Dictionary<string, RestExtensionMethodInfo>();

		bool _allowAll;
		string[] _allowGroups;
		string[] _allowTypes;
		string[] _allowMembers;
		MethodInfo _method;

		public bool Exists { get; private set; }
		public bool ReturnXml { get; private set; }

		#region Discovery

		public static RestExtensionMethodInfo GetMethod(string extensionAlias, string methodName)
		{
			return GetFromConfiguration(extensionAlias, methodName)
				?? GetFromAttribute(extensionAlias, methodName)
				?? GetFromLegacyAttribute(extensionAlias, methodName)
				?? MissingMethod;
		}

		static RestExtensionMethodInfo GetFromConfiguration(string extensionAlias, string methodName)
		{
			const string ExtensionXPath = "/RestExtensions/ext [@alias='{0}']";
			const string MethodXPath = "./permission [@method='{0}']";

			// fixme
			// because we reload the config file each time we can't really have a cache here
			// or we'd need to set a watcher on the config file?

			var doc = new XmlDocument();
			doc.Load(IOHelper.MapPath(SystemFiles.RestextensionsConfig));

			var eNode = doc.SelectSingleNode(string.Format(ExtensionXPath, extensionAlias));

			if (eNode == null)
				return null; // does not exist

			var mNode = eNode.SelectSingleNode(string.Format(MethodXPath, methodName));

			if (mNode == null)
				return null; // does not exist

			string assemblyName = eNode.Attributes["assembly"].Value;
			string assemblyPath = IOHelper.MapPath(string.Format("{0}/{1}.dll", SystemDirectories.Bin, assemblyName.TrimStart('/')));
			Assembly assembly = Assembly.LoadFrom(assemblyPath);

			string typeName = eNode.Attributes["type"].Value;
			Type type = assembly.GetType(typeName);

			var method = type.GetMethod(methodName);

			if (method == null)
				return null; // does not exist

			var allowAll = GetAttribute(mNode, "allowAll");
			var returnXml = GetAttribute(mNode, "returnXml");

			var info = new RestExtensionMethodInfo(allowAll != null && allowAll.ToLower() == "true",
				GetAttribute(mNode, "allowGroup"), GetAttribute(mNode, "allowType"), GetAttribute(mNode, "allowMember"),
				returnXml == null || returnXml.ToLower() != "false",
				method);

			return info;
		}

		static RestExtensionMethodInfo GetFromLegacyAttribute(string extensionAlias, string methodName)
		{
			// here we can cache because any change would trigger an app restart

			string cacheKey = extensionAlias + "." + methodName;
			lock (_cache)
			{
				if (_cache.ContainsKey(cacheKey))
					return _cache[cacheKey];
			}

			var extensions = PluginManager.Current.ResolveLegacyRestExtensions();
			var extension = extensions
				.SingleOrDefault(type => type.GetCustomAttribute<global::umbraco.presentation.umbracobase.RestExtension>(false).GetAlias() == extensionAlias);

			RestExtensionMethodInfo info = null;

			if (extension != null)
			{
				var method = extension.GetMethod(methodName);
				if (method != null)
				{
					var attribute = method.GetCustomAttributes(typeof(global::umbraco.presentation.umbracobase.RestExtensionMethod), false).Cast<global::umbraco.presentation.umbracobase.RestExtensionMethod>().SingleOrDefault();
					if (attribute != null)
					{
						info = new RestExtensionMethodInfo(attribute.GetAllowAll(),
							attribute.GetAllowGroup(), attribute.GetAllowType(), attribute.GetAllowMember(),
							attribute.returnXml,
							method);
					}
				}
			}

			lock (_cache)
			{
				_cache[cacheKey] = info;
			}

			return info;
		}

		static RestExtensionMethodInfo GetFromAttribute(string extensionAlias, string methodName)
		{
			// here we can cache because any change would trigger an app restart

			string cacheKey = extensionAlias + "." + methodName;
			lock (_cache)
			{
				if (_cache.ContainsKey(cacheKey))
					return _cache[cacheKey];
			}

			var extensions = PluginManager.Current.ResolveRestExtensions();
			var extension = extensions
				.SingleOrDefault(type => type.GetCustomAttribute<RestExtensionAttribute>(false).Alias == extensionAlias);

			RestExtensionMethodInfo info = null;

			if (extension != null)
			{
				var method = extension.GetMethod(methodName);
				if (method != null)
				{
					var attribute = method.GetCustomAttributes(typeof(RestExtensionMethodAttribute), false).Cast<RestExtensionMethodAttribute>().SingleOrDefault();
					if (attribute != null)
					{
						info = new RestExtensionMethodInfo(attribute.AllowAll,
							attribute.AllowGroup, attribute.AllowType, attribute.AllowMember,
							attribute.ReturnXml,
							method);
					}
				}
			}

			lock (_cache)
			{
				_cache[cacheKey] = info;
			}

			return info;
		}

		#endregion

		#region Invoke

		public bool CanBeInvokedByCurrentMember
		{
			get
			{
				if (_allowAll)
					return true;

				var member = Member.GetCurrentMember();

				if (member == null)
					return false;

				bool allowed = false;

				if (_allowGroups.Length > 0)
				{
					// fixme - are these equivalent?
					//var groups = member.Groups.Values.Cast<MemberGroup>().Select(group => group.Text);
					var groups = System.Web.Security.Roles.GetRolesForUser(member.LoginName);
					allowed = groups.Select(s => s.ToLower()).Intersect(_allowGroups).Any();
				}

				if (!allowed && _allowTypes.Length > 0)
				{
					allowed = _allowTypes.Contains(member.ContentType.Alias);
				}

				if (!allowed && _allowMembers.Length > 0)
				{
					allowed = _allowMembers.Contains(member.Id.ToString());
				}

				return allowed;
			}
		}

		public string Invoke(string[] parameters)
		{
			try
			{
				if (!_method.IsPublic || !_method.IsStatic)
				{
					// ensure that method is static public
					return "<error>Method has to be public and static</error>";
				}
				else
				{
					// ensure we have the right number of parameters
					if (_method.GetParameters().Length != parameters.Length)
					{
						return "<error>Not Enough parameters in url</error>";
					}
					else
					{
						// invoke

						// fixme - what is the point?
						//Create an instance of the type we need to invoke the method from.
						//**Object obj = Activator.CreateInstance(myExtension.type);

						object response;

						if (_method.GetParameters().Length == 0)
						{
							//response = myMethod.method.Invoke(obj, BindingFlags.Public | BindingFlags.Instance, bBinder, null, System.Globalization.CultureInfo.CurrentCulture);
							response = _method.Invoke(null /*obj*/, null); // invoke with null as parameters as there are none
						}

						else
						{
							object[] methodParams = new object[parameters.Length];

							int i = 0;

							foreach (ParameterInfo pInfo in _method.GetParameters())
							{
								Type myType = Type.GetType(pInfo.ParameterType.ToString());
								methodParams[(i)] = Convert.ChangeType(parameters[i], myType);
								i++;
							}

							//Invoke with methodParams
							//response = myMethod.method.Invoke(obj, BindingFlags.Public | BindingFlags.Instance, bBinder, methodParams, System.Globalization.CultureInfo.CurrentCulture);
							response = _method.Invoke(/*obj*/null, methodParams);
						}

						/*TODO - SOMETHING ALITTLE BETTER THEN ONLY CHECK FOR XPATHNODEITERATOR OR ELSE do ToString() */
						if (response != null)
						{
							switch (_method.ReturnType.ToString())
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

									if (this.ReturnXml)
									{
										// do a quick "is this html?" check... if it is add CDATA... 
										if (strResponse.Contains("<") || strResponse.Contains(">"))
											strResponse = "<![CDATA[" + strResponse + "]]>";
										return "<value>" + strResponse + "</value>";
									}
									else
									{
										//HttpContext.Current.Response.ContentType = "text/html";
										return strResponse;
									}
							}
						}
						else
						{
							if (this.ReturnXml)
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

		#endregion
	}
}
