using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using umbraco.interfaces;
using System.Reflection;
using System.Xml.Linq;

namespace Umbraco.Core.Dynamics
{

	/// <summary>
	/// The dynamic model for views
	/// </summary>
	public class DynamicPublishedContent : DynamicObject, IPublishedContent
	{
		private readonly IPublishedContent _publishedContent;
		private DynamicDocumentList _cachedChildren;
		private readonly ConcurrentDictionary<string, object> _cachedMemberOutput = new ConcurrentDictionary<string, object>();

		internal DynamicDocumentList OwnerList { get; set; }

		#region Constructors

		public DynamicPublishedContent(IPublishedContent node)
		{
			if (node == null) throw new ArgumentNullException("node");
			_publishedContent = node;
		}

		/// <summary>
		/// Returns an empty/blank DynamicPublishedContent, this is used for special case scenarios
		/// </summary>
		/// <returns></returns>
		internal static DynamicPublishedContent Empty()
		{
			return new DynamicPublishedContent();
		}

		private DynamicPublishedContent()
		{
		} 

		#endregion

		public dynamic AsDynamic()
		{
			return this;
		}

		#region Traversal
		public DynamicPublishedContent Up()
		{
			return DynamicDocumentWalker.Up(this);
		}
		public DynamicPublishedContent Up(int number)
		{
			return DynamicDocumentWalker.Up(this, number);
		}
		public DynamicPublishedContent Up(string nodeTypeAlias)
		{
			return DynamicDocumentWalker.Up(this, nodeTypeAlias);
		}
		public DynamicPublishedContent Down()
		{
			return DynamicDocumentWalker.Down(this);
		}
		public DynamicPublishedContent Down(int number)
		{
			return DynamicDocumentWalker.Down(this, number);
		}
		public DynamicPublishedContent Down(string nodeTypeAlias)
		{
			return DynamicDocumentWalker.Down(this, nodeTypeAlias);
		}
		public DynamicPublishedContent Next()
		{
			return DynamicDocumentWalker.Next(this);
		}
		public DynamicPublishedContent Next(int number)
		{
			return DynamicDocumentWalker.Next(this, number);
		}
		public DynamicPublishedContent Next(string nodeTypeAlias)
		{
			return DynamicDocumentWalker.Next(this, nodeTypeAlias);
		}

		public DynamicPublishedContent Previous()
		{
			return DynamicDocumentWalker.Previous(this);
		}
		public DynamicPublishedContent Previous(int number)
		{
			return DynamicDocumentWalker.Previous(this, number);
		}
		public DynamicPublishedContent Previous(string nodeTypeAlias)
		{
			return DynamicDocumentWalker.Previous(this, nodeTypeAlias);
		}
		public DynamicPublishedContent Sibling(int number)
		{
			return DynamicDocumentWalker.Sibling(this, number);
		}
		public DynamicPublishedContent Sibling(string nodeTypeAlias)
		{
			return DynamicDocumentWalker.Sibling(this, nodeTypeAlias);
		} 
		#endregion

		public bool HasProperty(string name)
		{
			if (_publishedContent != null)
			{
				try
				{
					var prop = GetUserProperty(name);

					return (prop != null);
				}
				catch (Exception)
				{
					return false;
				}
			}
			return false;
		}

		/// <summary>
		/// Attempts to call a method on the dynamic object
		/// </summary>
		/// <param name="binder"></param>
		/// <param name="args"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			//TODO: We MUST cache the result here, it is very expensive to keep finding extension methods!

			try
			{
				//Property?
				result = typeof(DynamicPublishedContent).InvokeMember(binder.Name,
												  System.Reflection.BindingFlags.Instance |
												  System.Reflection.BindingFlags.Public |
												  System.Reflection.BindingFlags.GetProperty,
												  null,
												  this,
												  args);
				return true;
			}
			catch (MissingMethodException)
			{
				try
				{
					//Static or Instance Method?
					result = typeof(DynamicPublishedContent).InvokeMember(binder.Name,
												  System.Reflection.BindingFlags.Instance |
												  System.Reflection.BindingFlags.Public |
												  System.Reflection.BindingFlags.Static |
												  System.Reflection.BindingFlags.InvokeMethod,
												  null,
												  this,
												  args);
					return true;
				}
				catch (MissingMethodException)
				{
					try
					{
						result = ExecuteExtensionMethod(args, binder.Name);
						return true;
					}
					catch (TargetInvocationException)
					{
						result = new DynamicNull();
						return true;
					}

					catch
					{
						//TODO: LOg this!

						result = null;
						return false;
					}

				}


			}
			catch
			{
				result = null;
				return false;
			}

		}

		private object ExecuteExtensionMethod(object[] args, string name)
		{
			object result = null;
			
			var methodTypesToFind = new[]
        		{
					typeof(DynamicPublishedContent)
        		};

			//find known extension methods that match the first type in the list
			MethodInfo toExecute = null;
			foreach (var t in methodTypesToFind)
			{
				toExecute = ExtensionMethodFinder.FindExtensionMethod(t, args, name, false);
				if (toExecute != null)
					break;
			}

			if (toExecute != null)
			{
				var genericArgs = (new[] { this }).Concat(args);
				result = toExecute.Invoke(null, genericArgs.ToArray());
			}
			else
			{
				throw new MissingMethodException();
			}
			if (result != null)
			{
				if (result is IPublishedContent)
				{
					result = new DynamicPublishedContent((IPublishedContent)result);
				}				
				if (result is IEnumerable<IPublishedContent>)
				{
					result = new DynamicDocumentList((IEnumerable<IPublishedContent>)result);
				}
				if (result is IEnumerable<DynamicPublishedContent>)
				{
					result = new DynamicDocumentList((IEnumerable<DynamicPublishedContent>)result);
				}				
			}
			return result;
		}

		/// <summary>
		/// Attempts to return a custom member (generally based on a string match)
		/// </summary>
		/// <param name="binder"></param>
		/// <returns></returns>
		protected virtual Attempt<object> TryGetCustomMember(GetMemberBinder binder)
		{
			if (binder.Name.InvariantEquals("ChildrenAsList") || binder.Name.InvariantEquals("Children"))
			{
				return new Attempt<object>(true, Children);
			}

			if (binder.Name.InvariantEquals("parentId"))
			{
				var parent = Parent;
				if (parent == null)
				{
					throw new InvalidOperationException(string.Format("The node {0} does not have a parent", Id));
				}
				return new Attempt<object>(true, parent.Id);
			}
			return Attempt<object>.False;
		}

		/// <summary>
		/// Attempts to return the children by the document type's alias (for example: CurrentPage.NewsItems where NewsItem is the
		/// document type alias)
		/// </summary>
		/// <param name="binder"></param>
		/// <returns></returns>
		/// <remarks>
		/// This method will work by both the plural and non-plural alias (i.e. NewsItem and NewsItems)
		/// </remarks>
		protected virtual Attempt<object> TryGetChildrenByAlias(GetMemberBinder binder)
		{
			
			var filteredTypeChildren = _publishedContent.Children
				.Where(x => x.DocumentTypeAlias.InvariantEquals(binder.Name) || x.DocumentTypeAlias.MakePluralName().InvariantEquals(binder.Name))
				.ToArray();
			if (filteredTypeChildren.Any())
			{
				return new Attempt<object>(true,
				                           new DynamicDocumentList(filteredTypeChildren.Select(x => new DynamicPublishedContent(x))));
			}
			return Attempt<object>.False;
		}

		/// <summary>
		/// Attempts to return a member based on the reflected document property
		/// </summary>
		/// <param name="binder"></param>
		/// <returns></returns>
		protected virtual Attempt<object> TryGetDocumentProperty(GetMemberBinder binder)
		{
			
			var reflectedProperty = GetReflectedProperty(binder.Name);
			var result = reflectedProperty != null
				? reflectedProperty.Value
				: null;

			return result == null
			       	? Attempt<object>.False
			       	: new Attempt<object>(true, result);			
		}

		/// <summary>
		/// Attempts to return a member based on a user defined umbraco property
		/// </summary>
		/// <param name="binder"></param>
		/// <returns></returns>
		protected virtual Attempt<object> TryGetUserProperty(GetMemberBinder binder)
		{
			var name = binder.Name;
			var recursive = false;
			if (name.StartsWith("_"))
			{
				name = name.Substring(1, name.Length - 1);
				recursive = true;
			}

			var userProperty = GetUserProperty(name, recursive);

			if (userProperty == null)
			{
				return Attempt<object>.False;
			}
			
			var result = userProperty.Value;

			if (_publishedContent.DocumentTypeAlias == null && userProperty.Alias == null)
			{
				throw new InvalidOperationException("No node alias or property alias available. Unable to look up the datatype of the property you are trying to fetch.");
			}

			//get the data type id for the current property
			var dataType = DynamicDocumentDataSourceResolver.Current.DataSource.GetDataType(userProperty.DocumentTypeAlias, userProperty.Alias);

			//convert the string value to a known type
			var converted = ConvertPropertyValue(result, dataType, userProperty.DocumentTypeAlias, userProperty.Alias);
			if (converted.Success)
			{
				result = converted.Result;
			}

			return new Attempt<object>(true, result);
		}

		/// <summary>
		/// Returns the member match methods in the correct order and is used in the TryGetMember method.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable<Func<GetMemberBinder, Attempt<object>>> GetMemberMatchMethods()
		{
			var memberMatchMethods = new List<Func<GetMemberBinder, Attempt<object>>>
				{
					b => TryGetCustomMember(b),		//match custom members
					b => TryGetUserProperty(b),		//then match custom user defined umbraco properties
					b => TryGetChildrenByAlias(b),	//then try to match children based on doc type alias
					b => TryGetDocumentProperty(b)  //then try to match on a reflected document property
				};
			return memberMatchMethods;
		} 

		/// <summary>
		/// Try to return an object based on the dynamic member accessor
		/// </summary>
		/// <param name="binder"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		/// <remarks>
		/// TODO: SD: This will alwasy return true so that no exceptions are generated, this is only because this is how the 
		/// old DynamicNode worked, I'm not sure if this is the correct/expected functionality but I've left it like that.
		/// IMO I think this is incorrect and it would be better to throw an exception for something that is not supported!
		/// </remarks>
		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			if (binder == null) throw new ArgumentNullException("binder");

			var name = binder.Name;

			//check the cache first!
			if (_cachedMemberOutput.TryGetValue(name, out result))
			{
				return true;
			}
			
			//loop through each member match method and execute it. 
			//If it is successful, cache the result and return it.
			foreach (var attempt in GetMemberMatchMethods()
				.Select(m => m(binder))
				.Where(attempt => attempt.Success))
			{
				result = attempt.Result;
				//cache the result so we don't have to re-process the whole thing
				_cachedMemberOutput.TryAdd(name, result);
				return true;
			}

			//if property access, type lookup and member invoke all failed
			//at this point, we're going to return null
			//instead, we return a DynamicNull - see comments in that file
			//this will let things like Model.ChildItem work and return nothing instead of crashing
			if (result == null)
			{
				//.Where explictly checks for this type
				//and will make it false
				//which means backwards equality (&& property != true) will pass
				//forwwards equality (&& property or && property == true) will fail
				result = new DynamicNull();
			}

			//alwasy return true if we haven't thrown an exception though I'm wondering if we return 'false' if .Net throws an exception for us??
			return true;
		}		

		/// <summary>
		/// Returns a property defined on the document object as a member property using reflection
		/// </summary>
		/// <param name="alias"></param>
		/// <returns></returns>
		private PropertyResult GetReflectedProperty(string alias)
		{
			return GetPropertyInternal(alias, _publishedContent, false);
		}

		/// <summary>
		/// Return a user defined property
		/// </summary>
		/// <param name="alias"></param>
		/// <param name="recursive"></param>
		/// <returns></returns>
		private PropertyResult GetUserProperty(string alias, bool recursive = false)
		{
			if (!recursive)
			{
				return GetPropertyInternal(alias, _publishedContent);
			}
			var context = this;
			var prop = GetPropertyInternal(alias, _publishedContent);
			while (prop == null || !prop.HasValue())
			{
				context = context.Parent;
				if (context == null) break;
				prop = context.GetPropertyInternal(alias, context._publishedContent);
			}
			return prop;
		}


		private PropertyResult GetPropertyInternal(string alias, IPublishedContent content, bool checkUserProperty = true)
		{
			if (alias.IsNullOrWhiteSpace()) throw new ArgumentNullException("alias");
			if (content == null) throw new ArgumentNullException("content");

			//if we're looking for a user defined property
			if (checkUserProperty)
			{
				var prop = content.GetProperty(alias);

				return prop == null
						? null
						: new PropertyResult(prop, PropertyResultType.UserProperty)
						{
							DocumentTypeAlias = content.DocumentTypeAlias,
							DocumentId = content.Id
						};
			}

			//reflect

			Func<string, Attempt<object>> getMember =
				memberAlias =>
				{
					try
					{
						return new Attempt<object>(true,
												   content.GetType().InvokeMember(memberAlias,
																				  System.Reflection.BindingFlags.GetProperty |
																				  System.Reflection.BindingFlags.Instance |
																				  System.Reflection.BindingFlags.Public,
																				  null,
																				  content,
																				  null));
					}
					catch (MissingMethodException ex)
					{
						return new Attempt<object>(ex);
					}
				};

			//try with the current casing
			var attempt = getMember(alias);
			if (!attempt.Success)
			{
				//if we cannot get with the current alias, try changing it's case
				attempt = alias[0].IsUpperCase()
					? getMember(alias.ConvertCase(StringAliasCaseType.CamelCase))
					: getMember(alias.ConvertCase(StringAliasCaseType.PascalCase));
			}

			return !attempt.Success
					? null
					: new PropertyResult(alias, attempt.Result, Guid.Empty, PropertyResultType.ReflectedProperty)
					{
						DocumentTypeAlias = content.DocumentTypeAlias,
						DocumentId = content.Id
					};
		}

		/// <summary>
		/// Converts the currentValue to a correctly typed value based on known registered converters, then based on known standards.
		/// </summary>
		/// <param name="currentValue"></param>
		/// <param name="dataType"></param>
		/// <param name="docTypeAlias"></param>
		/// <param name="propertyTypeAlias"></param>
		/// <returns></returns>
		private Attempt<object> ConvertPropertyValue(object currentValue, Guid dataType, string docTypeAlias, string propertyTypeAlias)
		{
			if (currentValue == null) return Attempt<object>.False;

			//First lets check all registered converters for this data type.			
			var converters = PropertyEditorValueConvertersResolver.Current.Converters
				.Where(x => x.IsConverterFor(dataType, docTypeAlias, propertyTypeAlias))
				.ToArray();

			//try to convert the value with any of the converters:
			foreach (var converted in converters
				.Select(p => p.ConvertPropertyValue(currentValue))
				.Where(converted => converted.Success))
			{
				return new Attempt<object>(true, converted.Result);
			}

			//if none of the converters worked, then we'll process this from what we know

			var sResult = Convert.ToString(currentValue).Trim();

			//this will eat csv strings, so only do it if the decimal also includes a decimal seperator (according to the current culture)
			if (sResult.Contains(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
			{
				decimal dResult;
				if (decimal.TryParse(sResult, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.CurrentCulture, out dResult))
				{
					return new Attempt<object>(true, dResult);
				}
			}
			//process string booleans as booleans
			if (sResult.InvariantEquals("true"))
			{
				return new Attempt<object>(true, true);
			}
			if (sResult.InvariantEquals("false"))
			{
				return new Attempt<object>(true, false);
			}

			//a really rough check to see if this may be valid xml
			//TODO: This is legacy code, I'm sure there's a better and nicer way
			if (sResult.StartsWith("<") && sResult.EndsWith(">") && sResult.Contains("/"))
			{
				try
				{
					var e = XElement.Parse(DynamicXml.StripDashesInElementOrAttributeNames(sResult), LoadOptions.None);

					//check that the document element is not one of the disallowed elements
					//allows RTE to still return as html if it's valid xhtml
					var documentElement = e.Name.LocalName;

					//TODO: See note against this setting, pretty sure we don't need this
					if (!UmbracoSettings.NotDynamicXmlDocumentElements.Any(
						tag => string.Equals(tag, documentElement, StringComparison.CurrentCultureIgnoreCase)))
					{
						return new Attempt<object>(true, new DynamicXml(e));
					}
					return Attempt<object>.False;
				}
				catch (Exception)
				{
					return Attempt<object>.False;
				}
			}
			return Attempt<object>.False;
		}

		//public DynamicNode Media(string propertyAlias)
		//{
		//    if (_n != null)
		//    {
		//        IProperty prop = _n.GetProperty(propertyAlias);
		//        if (prop != null)
		//        {
		//            int mediaNodeId;
		//            if (int.TryParse(prop.Value, out mediaNodeId))
		//            {
		//                return _razorLibrary.Value.MediaById(mediaNodeId);
		//            }
		//        }
		//        return null;
		//    }
		//    return null;
		//}
		//public bool IsProtected
		//{
		//    get
		//    {
		//        if (_n != null)
		//        {
		//            return umbraco.library.IsProtected(_n.Id, _n.Path);
		//        }
		//        return false;
		//    }
		//}
		//public bool HasAccess
		//{
		//    get
		//    {
		//        if (_n != null)
		//        {
		//            return umbraco.library.HasAccess(_n.Id, _n.Path);
		//        }
		//        return true;
		//    }
		//}

		//public string Media(string propertyAlias, string mediaPropertyAlias)
		//{
		//    if (_n != null)
		//    {
		//        IProperty prop = _n.GetProperty(propertyAlias);
		//        if (prop == null && propertyAlias.Substring(0, 1).ToUpper() == propertyAlias.Substring(0, 1))
		//        {
		//            prop = _n.GetProperty(propertyAlias.Substring(0, 1).ToLower() + propertyAlias.Substring((1)));
		//        }
		//        if (prop != null)
		//        {
		//            int mediaNodeId;
		//            if (int.TryParse(prop.Value, out mediaNodeId))
		//            {
		//                umbraco.cms.businesslogic.media.Media media = new cms.businesslogic.media.Media(mediaNodeId);
		//                if (media != null)
		//                {
		//                    Property mprop = media.getProperty(mediaPropertyAlias);
		//                    // check for nicer support of Pascal Casing EVEN if alias is camelCasing:
		//                    if (prop == null && mediaPropertyAlias.Substring(0, 1).ToUpper() == mediaPropertyAlias.Substring(0, 1))
		//                    {
		//                        mprop = media.getProperty(mediaPropertyAlias.Substring(0, 1).ToLower() + mediaPropertyAlias.Substring((1)));
		//                    }
		//                    if (mprop != null)
		//                    {
		//                        return string.Format("{0}", mprop.Value);
		//                    }
		//                }
		//            }
		//        }
		//    }
		//    return null;
		//}

		#region Ancestors, Descendants and Parent
		public DynamicPublishedContent AncestorOrSelf()
		{
			//TODO: Why is this query like this??
			return AncestorOrSelf(node => node.Level == 1);
		}
		public DynamicPublishedContent AncestorOrSelf(int level)
		{
			return AncestorOrSelf(node => node.Level == level);
		}
		public DynamicPublishedContent AncestorOrSelf(string nodeTypeAlias)
		{
			return AncestorOrSelf(node => node.DocumentTypeAlias == nodeTypeAlias);
		}
		public DynamicPublishedContent AncestorOrSelf(Func<DynamicPublishedContent, bool> func)
		{
			var node = this;
			while (node != null)
			{
				if (func(node)) return node;
				DynamicPublishedContent parent = node.Parent;
				if (parent != null)
				{
					if (this != parent)
					{
						node = parent;
					}
					else
					{
						return node;
					}
				}
				else
				{
					return null;
				}
			}
			return node;
		}
		public DynamicDocumentList AncestorsOrSelf(Func<DynamicPublishedContent, bool> func)
		{
			var ancestorList = new List<DynamicPublishedContent>();
			var node = this;
			ancestorList.Add(node);
			while (node != null)
			{
				if (node.Level == 1) break;
				DynamicPublishedContent parent = node.Parent;
				if (parent != null)
				{
					if (this != parent)
					{
						node = parent;
						if (func(node))
						{
							ancestorList.Add(node);
						}
					}
					else
					{
						break;
					}
				}
				else
				{
					break;
				}
			}
			ancestorList.Reverse();
			return new DynamicDocumentList(ancestorList);
		}
		public DynamicDocumentList AncestorsOrSelf()
		{
			return AncestorsOrSelf(n => true);
		}
		public DynamicDocumentList AncestorsOrSelf(string nodeTypeAlias)
		{
			return AncestorsOrSelf(n => n.DocumentTypeAlias == nodeTypeAlias);
		}
		public DynamicDocumentList AncestorsOrSelf(int level)
		{
			return AncestorsOrSelf(n => n.Level <= level);
		}
		public DynamicDocumentList Descendants(string nodeTypeAlias)
		{
			return Descendants(p => p.DocumentTypeAlias == nodeTypeAlias);
		}
		public DynamicDocumentList Descendants(int level)
		{
			return Descendants(p => p.Level >= level);
		}
		public DynamicDocumentList Descendants()
		{
			return Descendants(n => true);
		}
		internal DynamicDocumentList Descendants(Func<IPublishedContent, bool> func)
		{
			var flattenedNodes = this._publishedContent.Children.Map(func, (IPublishedContent n) => n.Children);
			return new DynamicDocumentList(flattenedNodes.ToList().ConvertAll(dynamicBackingItem => new DynamicPublishedContent(dynamicBackingItem)));
		}
		public DynamicDocumentList DescendantsOrSelf(int level)
		{
			return DescendantsOrSelf(p => p.Level >= level);
		}
		public DynamicDocumentList DescendantsOrSelf(string nodeTypeAlias)
		{
			return DescendantsOrSelf(p => p.DocumentTypeAlias == nodeTypeAlias);
		}
		public DynamicDocumentList DescendantsOrSelf()
		{
			return DescendantsOrSelf(p => true);
		}
		internal DynamicDocumentList DescendantsOrSelf(Func<IPublishedContent, bool> func)
		{
			if (this._publishedContent != null)
			{
				var thisNode = new List<IPublishedContent>();
				if (func(this._publishedContent))
				{
					thisNode.Add(this._publishedContent);
				}
				var flattenedNodes = this._publishedContent.Children.Map(func, (IPublishedContent n) => n.Children);
				return new DynamicDocumentList(thisNode.Concat(flattenedNodes).ToList().ConvertAll(dynamicBackingItem => new DynamicPublishedContent(dynamicBackingItem)));
			}
			return new DynamicDocumentList(Enumerable.Empty<IPublishedContent>());
		}
		public DynamicDocumentList Ancestors(int level)
		{
			return Ancestors(n => n.Level <= level);
		}
		public DynamicDocumentList Ancestors(string nodeTypeAlias)
		{
			return Ancestors(n => n.DocumentTypeAlias == nodeTypeAlias);
		}
		public DynamicDocumentList Ancestors()
		{
			return Ancestors(n => true);
		}
		public DynamicDocumentList Ancestors(Func<DynamicPublishedContent, bool> func)
		{
			var ancestorList = new List<DynamicPublishedContent>();
			var node = this;
			while (node != null)
			{
				if (node.Level == 1) break;
				DynamicPublishedContent parent = node.Parent;
				if (parent != null)
				{
					if (this != parent)
					{
						node = parent;
						if (func(node))
						{
							ancestorList.Add(node);
						}
					}
					else
					{
						break;
					}
				}
				else
				{
					break;
				}
			}
			ancestorList.Reverse();
			return new DynamicDocumentList(ancestorList);
		}
		public DynamicPublishedContent Parent
		{
			get
			{
				if (_publishedContent.Parent != null)
				{
					return new DynamicPublishedContent(_publishedContent.Parent);
				}
				if (_publishedContent != null && _publishedContent.Id == 0)
				{
					return this;
				}
				return null;
			}
		} 
		#endregion

		public int TemplateId
		{
			get { return _publishedContent.TemplateId; }
		}

		public int SortOrder
		{
			get { return _publishedContent.SortOrder; }
		}

		public string Name
		{
			get { return _publishedContent.Name; }
		}

		public bool Visible
		{
			get
			{

				var umbracoNaviHide = GetUserProperty("umbracoNaviHide");
				if (umbracoNaviHide != null)
				{
					return umbracoNaviHide.Value.ToString().Trim() != "1";
				}
				return true;
			}
		}

		public string UrlName
		{
			get { return _publishedContent.UrlName; }
		}

		public string DocumentTypeAlias
		{
			get { return _publishedContent.DocumentTypeAlias; }
		}

		public string WriterName
		{
			get { return _publishedContent.WriterName; }
		}

		public string CreatorName
		{
			get { return _publishedContent.CreatorName; }
		}

		public int WriterId
		{
			get { return _publishedContent.WriterId; }
		}

		public int CreatorId
		{
			get { return _publishedContent.CreatorId; }
		}

		public string Path
		{
			get { return _publishedContent.Path; }
		}

		public DateTime CreateDate
		{
			get { return _publishedContent.CreateDate; }
		}
		
		public int Id
		{
			get { return _publishedContent.Id; }
		}

		public DateTime UpdateDate
		{
			get { return _publishedContent.UpdateDate; }
		}

		public Guid Version
		{
			get { return _publishedContent.Version; }
		}
		
		public int Level
		{
			get { return _publishedContent.Level; }
		}

		public IEnumerable<IDocumentProperty> Properties
		{
			get { return _publishedContent.Properties; }
		}
		
		public IEnumerable<DynamicPublishedContent> Children
		{
			get
			{
				if (_cachedChildren == null)
				{
					var children = _publishedContent.Children;
					//testing, think this must be a special case for the root node ?
					if (!children.Any() && _publishedContent.Id == 0)
					{
						_cachedChildren = new DynamicDocumentList(new List<DynamicPublishedContent> { new DynamicPublishedContent(this._publishedContent) });
					}
					else
					{
						_cachedChildren = new DynamicDocumentList(_publishedContent.Children.Select(x => new DynamicPublishedContent(x)));
					}
				}
				return _cachedChildren;
			}
		}

		#region GetProperty methods which can be used with the dynamic object

		public IDocumentProperty GetProperty(string alias)
		{
			return GetProperty(alias, false);
		}
		public IDocumentProperty GetProperty(string alias, bool recursive)
		{
			return alias.StartsWith("@")
				? GetReflectedProperty(alias.TrimStart('@'))
				: GetUserProperty(alias, recursive);
		}
		public string GetPropertyValue(string alias)
		{
			return GetPropertyValue(alias, false);
		}
		public string GetPropertyValue(string alias, string fallback)
		{
			var prop = GetPropertyValue(alias);
			return !prop.IsNullOrWhiteSpace() ? prop : fallback;
		}
		public string GetPropertyValue(string alias, bool recursive)
		{
			var p = alias.StartsWith("@")
					? GetReflectedProperty(alias.TrimStart('@'))
					: GetUserProperty(alias, recursive);
			return p == null ? null : p.ValueAsString;
		}
		public string GetPropertyValue(string alias, bool recursive, string fallback)
		{
			var prop = GetPropertyValue(alias, recursive);
			return !prop.IsNullOrWhiteSpace() ? prop : fallback;
		}

		#endregion
		
		#region HasValue
		public bool HasValue(string alias)
		{
			return HasValue(alias, false);
		}
		public bool HasValue(string alias, bool recursive)
		{
			var prop = GetUserProperty(alias, recursive);
			if (prop == null) return false;
			return prop.HasValue();
		}
		public IHtmlString HasValue(string alias, string valueIfTrue, string valueIfFalse)
		{
			return HasValue(alias, false) ? new HtmlString(valueIfTrue) : new HtmlString(valueIfFalse);
		}
		public IHtmlString HasValue(string alias, bool recursive, string valueIfTrue, string valueIfFalse)
		{
			return HasValue(alias, recursive) ? new HtmlString(valueIfTrue) : new HtmlString(valueIfFalse);
		}
		public IHtmlString HasValue(string alias, string valueIfTrue)
		{
			return HasValue(alias, false) ? new HtmlString(valueIfTrue) : new HtmlString(string.Empty);
		}
		public IHtmlString HasValue(string alias, bool recursive, string valueIfTrue)
		{
			return HasValue(alias, recursive) ? new HtmlString(valueIfTrue) : new HtmlString(string.Empty);
		} 
		#endregion

		public int Position()
		{
			return this.Index();
		}
		public int Index()
		{
			if (this.OwnerList == null && this.Parent != null)
			{
				//var list = this.Parent.Children.Select(n => new DynamicNode(n));
				var list = this.Parent.Children;
				this.OwnerList = new DynamicDocumentList(list);
			}
			if (this.OwnerList != null)
			{
				List<DynamicPublishedContent> container = this.OwnerList.Items.ToList();
				int currentIndex = container.FindIndex(n => n.Id == this.Id);
				if (currentIndex != -1)
				{
					return currentIndex;
				}
				else
				{
					throw new IndexOutOfRangeException(string.Format("Node {0} belongs to a DynamicDocumentList but could not retrieve the index for it's position in the list", this.Id));
				}
			}
			else
			{
				throw new ArgumentNullException(string.Format("Node {0} has been orphaned and doesn't belong to a DynamicDocumentList", this.Id));
			}
		}

		#region Is Helpers
		public bool IsNull(string alias, bool recursive)
		{
			var prop = GetUserProperty(alias, recursive);
			if (prop == null) return true;
			return ((PropertyResult)prop).HasValue();
		}
		public bool IsNull(string alias)
		{
			return IsNull(alias, false);
		}
		public bool IsFirst()
		{
			return IsHelper(n => n.Index() == 0);
		}
		public HtmlString IsFirst(string valueIfTrue)
		{
			return IsHelper(n => n.Index() == 0, valueIfTrue);
		}
		public HtmlString IsFirst(string valueIfTrue, string valueIfFalse)
		{
			return IsHelper(n => n.Index() == 0, valueIfTrue, valueIfFalse);
		}
		public bool IsNotFirst()
		{
			return !IsHelper(n => n.Index() == 0);
		}
		public HtmlString IsNotFirst(string valueIfTrue)
		{
			return IsHelper(n => n.Index() != 0, valueIfTrue);
		}
		public HtmlString IsNotFirst(string valueIfTrue, string valueIfFalse)
		{
			return IsHelper(n => n.Index() != 0, valueIfTrue, valueIfFalse);
		}
		public bool IsPosition(int index)
		{
			if (this.OwnerList == null)
			{
				return false;
			}
			return IsHelper(n => n.Index() == index);
		}
		public HtmlString IsPosition(int index, string valueIfTrue)
		{
			if (this.OwnerList == null)
			{
				return new HtmlString(string.Empty);
			}
			return IsHelper(n => n.Index() == index, valueIfTrue);
		}
		public HtmlString IsPosition(int index, string valueIfTrue, string valueIfFalse)
		{
			if (this.OwnerList == null)
			{
				return new HtmlString(valueIfFalse);
			}
			return IsHelper(n => n.Index() == index, valueIfTrue, valueIfFalse);
		}
		public bool IsModZero(int modulus)
		{
			if (this.OwnerList == null)
			{
				return false;
			}
			return IsHelper(n => n.Index() % modulus == 0);
		}
		public HtmlString IsModZero(int modulus, string valueIfTrue)
		{
			if (this.OwnerList == null)
			{
				return new HtmlString(string.Empty);
			}
			return IsHelper(n => n.Index() % modulus == 0, valueIfTrue);
		}
		public HtmlString IsModZero(int modulus, string valueIfTrue, string valueIfFalse)
		{
			if (this.OwnerList == null)
			{
				return new HtmlString(valueIfFalse);
			}
			return IsHelper(n => n.Index() % modulus == 0, valueIfTrue, valueIfFalse);
		}

		public bool IsNotModZero(int modulus)
		{
			if (this.OwnerList == null)
			{
				return false;
			}
			return IsHelper(n => n.Index() % modulus != 0);
		}
		public HtmlString IsNotModZero(int modulus, string valueIfTrue)
		{
			if (this.OwnerList == null)
			{
				return new HtmlString(string.Empty);
			}
			return IsHelper(n => n.Index() % modulus != 0, valueIfTrue);
		}
		public HtmlString IsNotModZero(int modulus, string valueIfTrue, string valueIfFalse)
		{
			if (this.OwnerList == null)
			{
				return new HtmlString(valueIfFalse);
			}
			return IsHelper(n => n.Index() % modulus != 0, valueIfTrue, valueIfFalse);
		}
		public bool IsNotPosition(int index)
		{
			if (this.OwnerList == null)
			{
				return false;
			}
			return !IsHelper(n => n.Index() == index);
		}
		public HtmlString IsNotPosition(int index, string valueIfTrue)
		{
			if (this.OwnerList == null)
			{
				return new HtmlString(string.Empty);
			}
			return IsHelper(n => n.Index() != index, valueIfTrue);
		}
		public HtmlString IsNotPosition(int index, string valueIfTrue, string valueIfFalse)
		{
			if (this.OwnerList == null)
			{
				return new HtmlString(valueIfFalse);
			}
			return IsHelper(n => n.Index() != index, valueIfTrue, valueIfFalse);
		}
		public bool IsLast()
		{
			if (this.OwnerList == null)
			{
				return false;
			}
			int count = this.OwnerList.Count();
			return IsHelper(n => n.Index() == count - 1);
		}
		public HtmlString IsLast(string valueIfTrue)
		{
			if (this.OwnerList == null)
			{
				return new HtmlString(string.Empty);
			}
			int count = this.OwnerList.Count();
			return IsHelper(n => n.Index() == count - 1, valueIfTrue);
		}
		public HtmlString IsLast(string valueIfTrue, string valueIfFalse)
		{
			if (this.OwnerList == null)
			{
				return new HtmlString(valueIfFalse);
			}
			int count = this.OwnerList.Count();
			return IsHelper(n => n.Index() == count - 1, valueIfTrue, valueIfFalse);
		}
		public bool IsNotLast()
		{
			if (this.OwnerList == null)
			{
				return false;
			}
			int count = this.OwnerList.Count();
			return !IsHelper(n => n.Index() == count - 1);
		}
		public HtmlString IsNotLast(string valueIfTrue)
		{
			if (this.OwnerList == null)
			{
				return new HtmlString(string.Empty);
			}
			int count = this.OwnerList.Count();
			return IsHelper(n => n.Index() != count - 1, valueIfTrue);
		}
		public HtmlString IsNotLast(string valueIfTrue, string valueIfFalse)
		{
			if (this.OwnerList == null)
			{
				return new HtmlString(valueIfFalse);
			}
			int count = this.OwnerList.Count();
			return IsHelper(n => n.Index() != count - 1, valueIfTrue, valueIfFalse);
		}
		public bool IsEven()
		{
			return IsHelper(n => n.Index() % 2 == 0);
		}
		public HtmlString IsEven(string valueIfTrue)
		{
			return IsHelper(n => n.Index() % 2 == 0, valueIfTrue);
		}
		public HtmlString IsEven(string valueIfTrue, string valueIfFalse)
		{
			return IsHelper(n => n.Index() % 2 == 0, valueIfTrue, valueIfFalse);
		}
		public bool IsOdd()
		{
			return IsHelper(n => n.Index() % 2 == 1);
		}
		public HtmlString IsOdd(string valueIfTrue)
		{
			return IsHelper(n => n.Index() % 2 == 1, valueIfTrue);
		}
		public HtmlString IsOdd(string valueIfTrue, string valueIfFalse)
		{
			return IsHelper(n => n.Index() % 2 == 1, valueIfTrue, valueIfFalse);
		}
		public bool IsEqual(DynamicPublishedContent other)
		{
			return IsHelper(n => n.Id == other.Id);
		}
		public HtmlString IsEqual(DynamicPublishedContent other, string valueIfTrue)
		{
			return IsHelper(n => n.Id == other.Id, valueIfTrue);
		}
		public HtmlString IsEqual(DynamicPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
			return IsHelper(n => n.Id == other.Id, valueIfTrue, valueIfFalse);
		}
		public bool IsNotEqual(DynamicPublishedContent other)
		{
			return IsHelper(n => n.Id != other.Id);
		}
		public HtmlString IsNotEqual(DynamicPublishedContent other, string valueIfTrue)
		{
			return IsHelper(n => n.Id != other.Id, valueIfTrue);
		}
		public HtmlString IsNotEqual(DynamicPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
			return IsHelper(n => n.Id != other.Id, valueIfTrue, valueIfFalse);
		}
		public bool IsDescendant(DynamicPublishedContent other)
		{
			var ancestors = this.Ancestors();
			return IsHelper(n => ancestors.Items.Find(ancestor => ancestor.Id == other.Id) != null);
		}
		public HtmlString IsDescendant(DynamicPublishedContent other, string valueIfTrue)
		{
			var ancestors = this.Ancestors();
			return IsHelper(n => ancestors.Items.Find(ancestor => ancestor.Id == other.Id) != null, valueIfTrue);
		}
		public HtmlString IsDescendant(DynamicPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
			var ancestors = this.Ancestors();
			return IsHelper(n => ancestors.Items.Find(ancestor => ancestor.Id == other.Id) != null, valueIfTrue, valueIfFalse);
		}
		public bool IsDescendantOrSelf(DynamicPublishedContent other)
		{
			var ancestors = this.AncestorsOrSelf();
			return IsHelper(n => ancestors.Items.Find(ancestor => ancestor.Id == other.Id) != null);
		}
		public HtmlString IsDescendantOrSelf(DynamicPublishedContent other, string valueIfTrue)
		{
			var ancestors = this.AncestorsOrSelf();
			return IsHelper(n => ancestors.Items.Find(ancestor => ancestor.Id == other.Id) != null, valueIfTrue);
		}
		public HtmlString IsDescendantOrSelf(DynamicPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
			var ancestors = this.AncestorsOrSelf();
			return IsHelper(n => ancestors.Items.Find(ancestor => ancestor.Id == other.Id) != null, valueIfTrue, valueIfFalse);
		}
		public bool IsAncestor(DynamicPublishedContent other)
		{
			var descendants = this.Descendants();
			return IsHelper(n => descendants.Items.Find(descendant => descendant.Id == other.Id) != null);
		}
		public HtmlString IsAncestor(DynamicPublishedContent other, string valueIfTrue)
		{
			var descendants = this.Descendants();
			return IsHelper(n => descendants.Items.Find(descendant => descendant.Id == other.Id) != null, valueIfTrue);
		}
		public HtmlString IsAncestor(DynamicPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
			var descendants = this.Descendants();
			return IsHelper(n => descendants.Items.Find(descendant => descendant.Id == other.Id) != null, valueIfTrue, valueIfFalse);
		}
		public bool IsAncestorOrSelf(DynamicPublishedContent other)
		{
			var descendants = this.DescendantsOrSelf();
			return IsHelper(n => descendants.Items.Find(descendant => descendant.Id == other.Id) != null);
		}
		public HtmlString IsAncestorOrSelf(DynamicPublishedContent other, string valueIfTrue)
		{
			var descendants = this.DescendantsOrSelf();
			return IsHelper(n => descendants.Items.Find(descendant => descendant.Id == other.Id) != null, valueIfTrue);
		}
		public HtmlString IsAncestorOrSelf(DynamicPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
			var descendants = this.DescendantsOrSelf();
			return IsHelper(n => descendants.Items.Find(descendant => descendant.Id == other.Id) != null, valueIfTrue, valueIfFalse);
		}
		public bool IsHelper(Func<DynamicPublishedContent, bool> test)
		{
			return test(this);
		}
		public HtmlString IsHelper(Func<DynamicPublishedContent, bool> test, string valueIfTrue)
		{
			return IsHelper(test, valueIfTrue, string.Empty);
		}
		public HtmlString IsHelper(Func<DynamicPublishedContent, bool> test, string valueIfTrue, string valueIfFalse)
		{
			return test(this) ? new HtmlString(valueIfTrue) : new HtmlString(valueIfFalse);
		} 
		#endregion

		#region Where

		public HtmlString Where(string predicate, string valueIfTrue)
		{
			return Where(predicate, valueIfTrue, string.Empty);
		}
		public HtmlString Where(string predicate, string valueIfTrue, string valueIfFalse)
		{
			if (Where(predicate))
			{
				return new HtmlString(valueIfTrue);
			}
			return new HtmlString(valueIfFalse);
		}
		public bool Where(string predicate)
		{
			//Totally gonna cheat here
			var dynamicDocumentList = new DynamicDocumentList();
			dynamicDocumentList.Add(this);
			var filtered = dynamicDocumentList.Where<DynamicPublishedContent>(predicate);
			if (Queryable.Count(filtered) == 1)
			{
				//this node matches the predicate
				return true;
			}
			return false;
		} 

		#endregion

		//TODO: need a method to return a string value for a user property regardless of xml content or data type thus bypassing all of the PropertyEditorValueConverters
		///// <summary>
		///// Returns the value as as string regardless of xml content or data type
		///// </summary>
		///// <returns></returns>
		//public override string ToString()
		//{
		//    return base.ToString();
		//}

		#region Explicit IPublishedContent implementation
		IPublishedContent IPublishedContent.Parent
		{
			get { return _publishedContent.Parent; }
		}

		int IPublishedContent.Id
		{
			get { return _publishedContent.Id; }
		}

		int IPublishedContent.TemplateId
		{
			get { return _publishedContent.TemplateId; }
		}

		int IPublishedContent.SortOrder
		{
			get { return _publishedContent.SortOrder; }
		}

		string IPublishedContent.Name
		{
			get { return _publishedContent.Name; }
		}

		string IPublishedContent.UrlName
		{
			get { return _publishedContent.UrlName; }
		}

		string IPublishedContent.DocumentTypeAlias
		{
			get { return _publishedContent.DocumentTypeAlias; }
		}

		int IPublishedContent.DocumentTypeId
		{
			get { return _publishedContent.DocumentTypeId; }
		}

		string IPublishedContent.WriterName
		{
			get { return _publishedContent.WriterName; }
		}

		string IPublishedContent.CreatorName
		{
			get { return _publishedContent.CreatorName; }
		}

		int IPublishedContent.WriterId
		{
			get { return _publishedContent.WriterId; }
		}

		int IPublishedContent.CreatorId
		{
			get { return _publishedContent.CreatorId; }
		}

		string IPublishedContent.Path
		{
			get { return _publishedContent.Path; }
		}

		DateTime IPublishedContent.CreateDate
		{
			get { return _publishedContent.CreateDate; }
		}

		DateTime IPublishedContent.UpdateDate
		{
			get { return _publishedContent.UpdateDate; }
		}

		Guid IPublishedContent.Version
		{
			get { return _publishedContent.Version; }
		}

		int IPublishedContent.Level
		{
			get { return _publishedContent.Level; }
		}

		System.Collections.ObjectModel.Collection<IDocumentProperty> IPublishedContent.Properties
		{
			get { return _publishedContent.Properties; }
		}

		IEnumerable<IPublishedContent> IPublishedContent.Children
		{
			get { return _publishedContent.Children; }
		}

		IDocumentProperty IPublishedContent.GetProperty(string alias)
		{
			return _publishedContent.GetProperty(alias);
		} 
		#endregion
	}
}
