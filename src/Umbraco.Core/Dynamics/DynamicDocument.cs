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
	public class DynamicDocument : DynamicObject
	{
		private readonly IDocument _document;
		private DynamicDocumentList _cachedChildren;
		private readonly ConcurrentDictionary<string, object> _cachedMemberOutput = new ConcurrentDictionary<string, object>();

		internal DynamicDocumentList OwnerList { get; set; }

		public DynamicDocument(IDocument node)
		{
			if (node == null) throw new ArgumentNullException("node");
			_document = node;
		}

		/// <summary>
		/// Returns an empty/blank DynamicDocument, this is used for special case scenarios
		/// </summary>
		/// <returns></returns>
		internal static DynamicDocument Empty()
		{
			return new DynamicDocument();
		}

		private DynamicDocument()
		{			
		}

		public dynamic AsDynamic()
		{
			return this;
		}

		public DynamicDocument Up()
		{
			return DynamicDocumentWalker.Up(this);
		}
		public DynamicDocument Up(int number)
		{
			return DynamicDocumentWalker.Up(this, number);
		}
		public DynamicDocument Up(string nodeTypeAlias)
		{
			return DynamicDocumentWalker.Up(this, nodeTypeAlias);
		}
		public DynamicDocument Down()
		{
			return DynamicDocumentWalker.Down(this);
		}
		public DynamicDocument Down(int number)
		{
			return DynamicDocumentWalker.Down(this, number);
		}
		public DynamicDocument Down(string nodeTypeAlias)
		{
			return DynamicDocumentWalker.Down(this, nodeTypeAlias);
		}
		public DynamicDocument Next()
		{
			return DynamicDocumentWalker.Next(this);
		}
		public DynamicDocument Next(int number)
		{
			return DynamicDocumentWalker.Next(this, number);
		}
		public DynamicDocument Next(string nodeTypeAlias)
		{
			return DynamicDocumentWalker.Next(this, nodeTypeAlias);
		}

		public DynamicDocument Previous()
		{
			return DynamicDocumentWalker.Previous(this);
		}
		public DynamicDocument Previous(int number)
		{
			return DynamicDocumentWalker.Previous(this, number);
		}
		public DynamicDocument Previous(string nodeTypeAlias)
		{
			return DynamicDocumentWalker.Previous(this, nodeTypeAlias);
		}
		public DynamicDocument Sibling(int number)
		{
			return DynamicDocumentWalker.Sibling(this, number);
		}
		public DynamicDocument Sibling(string nodeTypeAlias)
		{
			return DynamicDocumentWalker.Sibling(this, nodeTypeAlias);
		}

		public bool HasProperty(string name)
		{
			if (_document != null)
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
				result = typeof(DynamicDocument).InvokeMember(binder.Name,
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
					result = typeof(DynamicDocument).InvokeMember(binder.Name,
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
					typeof(DynamicDocument)
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
				if (result is IDocument)
				{
					result = new DynamicDocument((IDocument)result);
				}				
				if (result is IEnumerable<IDocument>)
				{
					result = new DynamicDocumentList((IEnumerable<IDocument>)result);
				}
				if (result is IEnumerable<DynamicDocument>)
				{
					result = new DynamicDocumentList((IEnumerable<DynamicDocument>)result);
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
			
			var filteredTypeChildren = _document.Children
				.Where(x => x.DocumentTypeAlias.InvariantEquals(binder.Name) || x.DocumentTypeAlias.MakePluralName().InvariantEquals(binder.Name))
				.ToArray();
			if (filteredTypeChildren.Any())
			{
				return new Attempt<object>(true,
				                           new DynamicDocumentList(filteredTypeChildren.Select(x => new DynamicDocument(x))));
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

			if (_document.DocumentTypeAlias == null && userProperty.Alias == null)
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
			return GetPropertyInternal(alias, _document, false);
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
				return GetPropertyInternal(alias, _document);
			}
			var context = this;
			var prop = GetPropertyInternal(alias, _document);
			while (prop == null || !prop.HasValue())
			{
				context = context.Parent;
				if (context == null) break;
				prop = context.GetPropertyInternal(alias, context._document);
			}
			return prop;
		}


		private PropertyResult GetPropertyInternal(string alias, IDocument content, bool checkUserProperty = true)
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

		public DynamicDocument AncestorOrSelf()
		{
			//TODO: Why is this query like this??
			return AncestorOrSelf(node => node.Level == 1);
		}
		public DynamicDocument AncestorOrSelf(int level)
		{
			return AncestorOrSelf(node => node.Level == level);
		}
		public DynamicDocument AncestorOrSelf(string nodeTypeAlias)
		{
			return AncestorOrSelf(node => node.DocumentTypeAlias == nodeTypeAlias);
		}
		public DynamicDocument AncestorOrSelf(Func<DynamicDocument, bool> func)
		{
			var node = this;
			while (node != null)
			{
				if (func(node)) return node;
				DynamicDocument parent = node.Parent;
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
		public DynamicDocumentList AncestorsOrSelf(Func<DynamicDocument, bool> func)
		{
			var ancestorList = new List<DynamicDocument>();
			var node = this;
			ancestorList.Add(node);
			while (node != null)
			{
				if (node.Level == 1) break;
				DynamicDocument parent = node.Parent;
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
		internal DynamicDocumentList Descendants(Func<IDocument, bool> func)
		{
			var flattenedNodes = this._document.Children.Map(func, (IDocument n) => n.Children);
			return new DynamicDocumentList(flattenedNodes.ToList().ConvertAll(dynamicBackingItem => new DynamicDocument(dynamicBackingItem)));
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
		internal DynamicDocumentList DescendantsOrSelf(Func<IDocument, bool> func)
		{
			if (this._document != null)
			{
				var thisNode = new List<IDocument>();
				if (func(this._document))
				{
					thisNode.Add(this._document);
				}
				var flattenedNodes = this._document.Children.Map(func, (IDocument n) => n.Children);
				return new DynamicDocumentList(thisNode.Concat(flattenedNodes).ToList().ConvertAll(dynamicBackingItem => new DynamicDocument(dynamicBackingItem)));
			}
			return new DynamicDocumentList(Enumerable.Empty<IDocument>());
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
		public DynamicDocumentList Ancestors(Func<DynamicDocument, bool> func)
		{
			var ancestorList = new List<DynamicDocument>();
			var node = this;
			while (node != null)
			{
				if (node.Level == 1) break;
				DynamicDocument parent = node.Parent;
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
		public DynamicDocument Parent
		{
			get
			{
				if (_document.Parent != null)
				{
					return new DynamicDocument(_document.Parent);
				}
				if (_document != null && _document.Id == 0)
				{
					return this;
				}
				return null;
			}
		}

		public int TemplateId
		{
			get { return _document.TemplateId; }
		}

		public int SortOrder
		{
			get { return _document.SortOrder; }
		}

		public string Name
		{
			get { return _document.Name; }
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
			get { return _document.UrlName; }
		}

		public string DocumentTypeAlias
		{
			get { return _document.DocumentTypeAlias; }
		}

		public string WriterName
		{
			get { return _document.WriterName; }
		}

		public string CreatorName
		{
			get { return _document.CreatorName; }
		}

		public int WriterId
		{
			get { return _document.WriterId; }
		}

		public int CreatorId
		{
			get { return _document.CreatorId; }
		}

		public string Path
		{
			get { return _document.Path; }
		}

		public DateTime CreateDate
		{
			get { return _document.CreateDate; }
		}
		public int Id
		{
			get { return _document.Id; }
		}

		public DateTime UpdateDate
		{
			get { return _document.UpdateDate; }
		}

		public Guid Version
		{
			get { return _document.Version; }
		}
		
		public int Level
		{
			get { return _document.Level; }
		}

		public IEnumerable<IDocumentProperty> Properties
		{
			get { return _document.Properties; }
		}
		
		public IEnumerable<DynamicDocument> Children
		{
			get
			{
				if (_cachedChildren == null)
				{
					var children = _document.Children;
					//testing, think this must be a special case for the root node ?
					if (!children.Any() && _document.Id == 0)
					{
						_cachedChildren = new DynamicDocumentList(new List<DynamicDocument> { new DynamicDocument(this._document) });
					}
					else
					{
						_cachedChildren = new DynamicDocumentList(_document.Children.Select(x => new DynamicDocument(x)));
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
				List<DynamicDocument> container = this.OwnerList.Items.ToList();
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
		public bool IsEqual(DynamicDocument other)
		{
			return IsHelper(n => n.Id == other.Id);
		}
		public HtmlString IsEqual(DynamicDocument other, string valueIfTrue)
		{
			return IsHelper(n => n.Id == other.Id, valueIfTrue);
		}
		public HtmlString IsEqual(DynamicDocument other, string valueIfTrue, string valueIfFalse)
		{
			return IsHelper(n => n.Id == other.Id, valueIfTrue, valueIfFalse);
		}
		public bool IsNotEqual(DynamicDocument other)
		{
			return IsHelper(n => n.Id != other.Id);
		}
		public HtmlString IsNotEqual(DynamicDocument other, string valueIfTrue)
		{
			return IsHelper(n => n.Id != other.Id, valueIfTrue);
		}
		public HtmlString IsNotEqual(DynamicDocument other, string valueIfTrue, string valueIfFalse)
		{
			return IsHelper(n => n.Id != other.Id, valueIfTrue, valueIfFalse);
		}
		public bool IsDescendant(DynamicDocument other)
		{
			var ancestors = this.Ancestors();
			return IsHelper(n => ancestors.Items.Find(ancestor => ancestor.Id == other.Id) != null);
		}
		public HtmlString IsDescendant(DynamicDocument other, string valueIfTrue)
		{
			var ancestors = this.Ancestors();
			return IsHelper(n => ancestors.Items.Find(ancestor => ancestor.Id == other.Id) != null, valueIfTrue);
		}
		public HtmlString IsDescendant(DynamicDocument other, string valueIfTrue, string valueIfFalse)
		{
			var ancestors = this.Ancestors();
			return IsHelper(n => ancestors.Items.Find(ancestor => ancestor.Id == other.Id) != null, valueIfTrue, valueIfFalse);
		}
		public bool IsDescendantOrSelf(DynamicDocument other)
		{
			var ancestors = this.AncestorsOrSelf();
			return IsHelper(n => ancestors.Items.Find(ancestor => ancestor.Id == other.Id) != null);
		}
		public HtmlString IsDescendantOrSelf(DynamicDocument other, string valueIfTrue)
		{
			var ancestors = this.AncestorsOrSelf();
			return IsHelper(n => ancestors.Items.Find(ancestor => ancestor.Id == other.Id) != null, valueIfTrue);
		}
		public HtmlString IsDescendantOrSelf(DynamicDocument other, string valueIfTrue, string valueIfFalse)
		{
			var ancestors = this.AncestorsOrSelf();
			return IsHelper(n => ancestors.Items.Find(ancestor => ancestor.Id == other.Id) != null, valueIfTrue, valueIfFalse);
		}
		public bool IsAncestor(DynamicDocument other)
		{
			var descendants = this.Descendants();
			return IsHelper(n => descendants.Items.Find(descendant => descendant.Id == other.Id) != null);
		}
		public HtmlString IsAncestor(DynamicDocument other, string valueIfTrue)
		{
			var descendants = this.Descendants();
			return IsHelper(n => descendants.Items.Find(descendant => descendant.Id == other.Id) != null, valueIfTrue);
		}
		public HtmlString IsAncestor(DynamicDocument other, string valueIfTrue, string valueIfFalse)
		{
			var descendants = this.Descendants();
			return IsHelper(n => descendants.Items.Find(descendant => descendant.Id == other.Id) != null, valueIfTrue, valueIfFalse);
		}
		public bool IsAncestorOrSelf(DynamicDocument other)
		{
			var descendants = this.DescendantsOrSelf();
			return IsHelper(n => descendants.Items.Find(descendant => descendant.Id == other.Id) != null);
		}
		public HtmlString IsAncestorOrSelf(DynamicDocument other, string valueIfTrue)
		{
			var descendants = this.DescendantsOrSelf();
			return IsHelper(n => descendants.Items.Find(descendant => descendant.Id == other.Id) != null, valueIfTrue);
		}
		public HtmlString IsAncestorOrSelf(DynamicDocument other, string valueIfTrue, string valueIfFalse)
		{
			var descendants = this.DescendantsOrSelf();
			return IsHelper(n => descendants.Items.Find(descendant => descendant.Id == other.Id) != null, valueIfTrue, valueIfFalse);
		}
		public bool IsHelper(Func<DynamicDocument, bool> test)
		{
			return test(this);
		}
		public HtmlString IsHelper(Func<DynamicDocument, bool> test, string valueIfTrue)
		{
			return IsHelper(test, valueIfTrue, string.Empty);
		}
		public HtmlString IsHelper(Func<DynamicDocument, bool> test, string valueIfTrue, string valueIfFalse)
		{
			return test(this) ? new HtmlString(valueIfTrue) : new HtmlString(valueIfFalse);
		}
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
			var filtered = dynamicDocumentList.Where<DynamicDocument>(predicate);
			if (Queryable.Count(filtered) == 1)
			{
				//this node matches the predicate
				return true;
			}
			return false;
		}

		/// <summary>
		/// Returns the value as as string regardless of xml content or data type
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return base.ToString();
		}
	}
}
