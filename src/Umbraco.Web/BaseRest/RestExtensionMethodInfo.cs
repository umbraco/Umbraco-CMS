﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

		static readonly char[] Split = new[] { ',' };

		static string[] SplitString(string s)
		{
		    return string.IsNullOrWhiteSpace(s) 
                ? new string[] { } 
                : s.ToLower().Split(Split, StringSplitOptions.RemoveEmptyEntries);
		}

	    static string GetAttribute(XmlNode node, string name)
		{
            if (node == null)
                throw new ArgumentNullException("node");
	        var attributes = node.Attributes;
            if (attributes == null)
                throw new ArgumentException(@"Node has no Attributes collection.", "node"); 
			var attribute = attributes[name];
			return attribute == null ? null : attribute.Value;
		}

		#endregion

		private RestExtensionMethodInfo()
		{
			Exists = false;
		}

		private RestExtensionMethodInfo(bool allowAll, string allowGroup, string allowType, string allowMember, bool returnXml, string contentType, MethodInfo method)
		{
			Exists = true;
			_allowAll = allowAll;
			_allowGroups = SplitString(allowGroup);
			_allowTypes = SplitString(allowType);
			_allowMembers = SplitString(allowMember);
			ReturnXml = returnXml;
			ContentType = contentType;
			_method = method;
		}

		static readonly RestExtensionMethodInfo MissingMethod = new RestExtensionMethodInfo();
		static readonly Dictionary<string, RestExtensionMethodInfo> Cache = new Dictionary<string, RestExtensionMethodInfo>();

	    readonly bool _allowAll;
	    readonly string[] _allowGroups;
	    readonly string[] _allowTypes;
	    readonly string[] _allowMembers;
	    readonly MethodInfo _method;

		public bool Exists { get; private set; }
		public bool ReturnXml { get; private set; }
		public string ContentType { get; private set; }

		#region Discovery

		// gets a RestExtensionMethodInfo matching extensionAlias and methodName
		// by looking everywhere (configuration, attributes, legacy attributes)
		// returns MissingMethod (ie .Exists == false) if not found
		//
		public static RestExtensionMethodInfo GetMethod(string extensionAlias, string methodName, int paramsCount)
		{
            // note - legacy does not support paramsCount

			return GetFromConfiguration(extensionAlias, methodName, paramsCount)
				?? GetFromAttribute(extensionAlias, methodName, paramsCount)
				?? GetFromLegacyConfiguration(extensionAlias, methodName) // that one should be obsoleted at some point
				?? GetFromLegacyAttribute(extensionAlias, methodName) // that one should be obsoleted at some point
				?? MissingMethod;
		}

		// gets a RestExtensionMethodInfo matching extensionAlias and methodName
		// by looking at the legacy configuration file
		// returns null if not found
		//
		static RestExtensionMethodInfo GetFromLegacyConfiguration(string extensionAlias, string methodName)
		{
			const string extensionXPath = "/RestExtensions/ext [@alias='{0}']";
			const string methodXPath = "./permission [@method='{0}']";

			var config = (Configuration.BaseRestSection)System.Configuration.ConfigurationManager.GetSection("BaseRestExtensions");

			if (config == null)
				return null; // does not exist

			// note - at the moment we reload the config file each time
			//   we have to support live edits of the config file for backward compatibility reason
			//   so if we want to cache, we'd also need to implement a watcher on the config file...

			var doc = new XmlDocument();
			doc.Load(IOHelper.MapPath(SystemFiles.RestextensionsConfig));

			var eNode = doc.SelectSingleNode(string.Format(extensionXPath, extensionAlias));

			if (eNode == null)
				return null; // does not exist

			var mNode = eNode.SelectSingleNode(string.Format(methodXPath, methodName));

			if (mNode == null)
				return null; // does not exist

		    var attributes = eNode.Attributes;
		    if (attributes == null)
		        return null; // has no attributes

			var assemblyName = attributes["assembly"].Value;
			var assembly = Assembly.Load(assemblyName);

			var typeName = attributes["type"].Value;
			var type = assembly.GetType(typeName);

			if (type == null)
				return null; // does not exist

			var method = type.GetMethod(methodName);

			if (method == null)
				return null; // does not exist

			var allowAll = GetAttribute(mNode, "allowAll");
			var returnXml = GetAttribute(mNode, "returnXml");
			var contentType = GetAttribute(mNode, "contentType");

			var info = new RestExtensionMethodInfo(allowAll != null && allowAll.ToLower() == "true",
				GetAttribute(mNode, "allowGroup"), GetAttribute(mNode, "allowType"), GetAttribute(mNode, "allowMember"),
				returnXml == null || returnXml.ToLower() != "false", contentType,
				method);

			return info;
		}

		// gets a RestExtensionMethodInfo matching extensionAlias and methodName
		// by looking at the configuration file
		// returns null if not found
		//
		static RestExtensionMethodInfo GetFromConfiguration(string extensionAlias, string methodName, int paramsCount)
		{
		    var config = Core.Configuration.UmbracoSettings.For<Configuration.BaseRestSection>();

			var configExtension = config.Items[extensionAlias];
			if (configExtension == null)
				return null; // does not exist

			var configMethod = configExtension[methodName];
			if (configMethod == null)
				return null; // does not exist

			MethodInfo method = null;
			try
			{
				var parts = configExtension.Type.Split(',');
				if (parts.Length > 2)
                    throw new Exception(string.Format("Failed to load extension '{0}', invalid type.", configExtension.Type));

				var assembly = parts.Length == 1 ? Assembly.GetExecutingAssembly() : Assembly.Load(parts[1]);
				var type = assembly.GetType(parts[0]);

			    if (type == null)
			        throw new Exception(string.Format("Could not get type \"{0}\".", parts[0]));
                
                var methods = type.GetMethods()
			                      .Where(m => m.Name == methodName)
			                      .Where(m => m.GetParameters().Count() == paramsCount)
                                  .ToArray();

			    if (methods.Length > 1)
			        throw new Exception(string.Format("Method \"{0}\" has many overloads with same number of parameters.", methodName));

                if (methods.Length > 0)
                {
                    method = methods[0];
                    if (!method.IsPublic || !method.IsStatic)
                        throw new Exception(string.Format("Method \"{0}\" has to be public and static.", methodName));
                }
			}
			catch (Exception e)
			{
				throw new Exception(string.Format("Failed to load extension '{0}', see inner exception.", configExtension.Type), e);
			}

			if (method == null)
				return null; // does not exist

			var info = new RestExtensionMethodInfo(configMethod.AllowAll,
				configMethod.AllowGroup, configMethod.AllowType, configMethod.AllowMember,
				configMethod.ReturnXml, configMethod.ContentType,
				method);

			return info;
		}

		// gets a RestExtensionMethodInfo matching extensionAlias and methodName
		// by looking for the legacy attributes
		// returns null if not found
		//
		static RestExtensionMethodInfo GetFromLegacyAttribute(string extensionAlias, string methodName)
		{
			// here we can cache because any change would trigger an app restart anyway

			var cacheKey = extensionAlias + "." + methodName;
			lock (Cache)
			{
				// if it's in the cache, return
				if (Cache.ContainsKey(cacheKey))
					return Cache[cacheKey];
			}

			// find an extension with that alias, then find a method with that name,
			// which has been properly marked with the attribute, and use the attribute
			// properties to setup a RestExtensionMethodInfo

            // note: add #pragma - yes it's obsolete but we still want to support it for the time being

			var extensions = PluginManager.Current.ResolveLegacyRestExtensions()
#pragma warning disable 612,618
				.Where(type => type.GetCustomAttribute<global::umbraco.presentation.umbracobase.RestExtension>(false).GetAlias() == extensionAlias);
#pragma warning restore 612,618

			RestExtensionMethodInfo info = null;

            foreach (var extension in extensions) // foreach classes with extension alias
			{
				var method = extension.GetMethod(methodName);
                if (method == null) continue; // not implementing the method = ignore

#pragma warning disable 612,618
			    var attribute = method.GetCustomAttributes(typeof(global::umbraco.presentation.umbracobase.RestExtensionMethod), false).Cast<global::umbraco.presentation.umbracobase.RestExtensionMethod>().SingleOrDefault();
#pragma warning restore 612,618
                if (attribute == null) continue; // method has not attribute = ignore

                // got it!
			    info = new RestExtensionMethodInfo(attribute.GetAllowAll(),
			                                       attribute.GetAllowGroup(), attribute.GetAllowType(), attribute.GetAllowMember(),
												   attribute.returnXml, attribute.contentType,
			                                       method);

			    // cache
			    lock (Cache)
			    {
			        Cache[cacheKey] = info;
			    }
            
                // got it, no need to look any further
                break;
            }

			return info;
		}

		// gets a RestExtensionMethodInfo matching extensionAlias and methodName
		// by looking for the attributes
		// returns null if not found
		//
		static RestExtensionMethodInfo GetFromAttribute(string extensionAlias, string methodName, int paramsCount)
		{
			// here we can cache because any change would trigger an app restart

		    var cacheKey = string.Format("{0}.{1}[{2}]", extensionAlias, methodName, paramsCount);
			lock (Cache)
			{
				// if it's in the cache, return
				if (Cache.ContainsKey(cacheKey))
					return Cache[cacheKey];
			}

			// find an extension with that alias, then find a method with that name,
			// which has been properly marked with the attribute, and use the attribute
			// properties to setup a RestExtensionMethodInfo
            //
            // note: the extension may be implemented by more than one class

			var extensions = PluginManager.Current.ResolveRestExtensions()
                .Where(type => type.GetCustomAttribute<RestExtensionAttribute>(false).Alias == extensionAlias);

			RestExtensionMethodInfo info = null;

            foreach (var extension in extensions) // foreach classes with extension alias
            {
                var methods = extension.GetMethods()
                                  .Where(m => m.Name == methodName)
                                  .Where(m => m.GetParameters().Count() == paramsCount)
                                  .ToArray();

                if (methods.Length == 0) continue; // not implementing the method = ignore

                if (methods.Length > 1)
                    throw new Exception(string.Format("Method \"{0}\" has many overloads with same number of parameters.", methodName));

                var method = methods[0];
                if (!method.IsPublic || !method.IsStatic)
                    throw new Exception(string.Format("Method \"{0}\" has to be public and static.", methodName));

                var attribute = method.GetCustomAttributes(typeof(RestExtensionMethodAttribute), false).Cast<RestExtensionMethodAttribute>().SingleOrDefault();
                if (attribute == null) continue; // method has not attribute = ignore

                // got it!
                info = new RestExtensionMethodInfo(attribute.AllowAll,
                                                   attribute.AllowGroup, attribute.AllowType, attribute.AllowMember,
												   attribute.ReturnXml, attribute.ContentType,
                                                   method);

                // cache
                lock (Cache)
                {
                    Cache[cacheKey] = info;
                }

                // got it, no need to look any further
                break;
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

				var allowed = false;

				if (_allowGroups.Length > 0)
				{
					// note - assuming these are equivalent
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
					allowed = _allowMembers.Contains(member.Id.ToString(CultureInfo.InvariantCulture));
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

				// ensure we have the right number of parameters
				if (_method.GetParameters().Length != parameters.Length)
				{
					return "<error>Not Enough parameters in url</error>";
				}

				// invoke

				object response;

				if (_method.GetParameters().Length == 0)
				{
					response = _method.Invoke(null, null); // invoke with null as parameters as there are none
				}
				else
				{
					var methodParams = new object[parameters.Length];

					var i = 0;

					foreach (var pInfo in _method.GetParameters())
					{
						var myType = Type.GetType(pInfo.ParameterType.ToString());
                        if (myType == null) throw new Exception("Failed to get type.");
						methodParams[(i)] = Convert.ChangeType(parameters[i], myType);
						i++;
					}

					response = _method.Invoke(null, methodParams);
				}

				// this is legacy and could probably be improved
				if (response != null)
				{
					switch (_method.ReturnType.ToString())
					{
						case "System.Xml.XPath.XPathNodeIterator":
							return ((System.Xml.XPath.XPathNodeIterator)response).Current.OuterXml;
						case "System.Xml.Linq.XDocument":
							return response.ToString();
						case "System.Xml.XmlDocument":
							var xmlDoc = (XmlDocument)response;
							var sw = new StringWriter();
							var xw = new XmlTextWriter(sw);
							xmlDoc.WriteTo(xw);
							return sw.ToString();
						default:
							var strResponse = response.ToString();

							if (ReturnXml)
							{
								// do a quick "is this html?" check... if it is add CDATA... 
								if (strResponse.Contains("<") || strResponse.Contains(">"))
									strResponse = "<![CDATA[" + strResponse + "]]>";
								return "<value>" + strResponse + "</value>";
							}
					        
                            return strResponse;
					}
				}
			    
                return ReturnXml ? "<error>Null value returned</error>" : string.Empty;
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
