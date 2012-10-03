using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using Umbraco.Core.Configuration;
using Umbraco.Core.Dynamics;
using Umbraco.Core.PropertyEditors;
using System.Reflection;
using System.Xml.Linq;

namespace Umbraco.Core.Models
{

	/// <summary>
	/// The base dynamic model for views
	/// </summary>
	public class DynamicPublishedContentBase : DynamicObject, IPublishedContent
	{
		protected IPublishedContent PublishedContent { get; private set; }
		private DynamicPublishedContentList _cachedChildren;
		private readonly ConcurrentDictionary<string, object> _cachedMemberOutput = new ConcurrentDictionary<string, object>();
		
		#region Constructors

		public DynamicPublishedContentBase(IPublishedContent node)
		{
			if (node == null) throw new ArgumentNullException("node");
			PublishedContent = node;
		}

		/// <summary>
		/// Returns an empty/blank DynamicPublishedContent, this is used for special case scenarios
		/// </summary>
		/// <returns></returns>
		internal static DynamicPublishedContentBase Empty()
		{
			return new DynamicPublishedContentBase();
		}

		private DynamicPublishedContentBase()
		{
		} 

		#endregion

		public dynamic AsDynamic()
		{
			return this;
		}

		public bool HasProperty(string name)
		{
			if (PublishedContent != null)
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
				result = typeof(DynamicPublishedContentBase).InvokeMember(binder.Name,
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
					result = typeof(DynamicPublishedContentBase).InvokeMember(binder.Name,
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
					typeof(DynamicPublishedContentBase)
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
					result = new DynamicPublishedContentBase((IPublishedContent)result);
				}				
				if (result is IEnumerable<IPublishedContent>)
				{
					result = new DynamicPublishedContentList((IEnumerable<IPublishedContent>)result);
				}
				if (result is IEnumerable<DynamicPublishedContentBase>)
				{
					result = new DynamicPublishedContentList((IEnumerable<DynamicPublishedContentBase>)result);
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
				var parent = ((IPublishedContent) this).Parent;
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
			
			var filteredTypeChildren = PublishedContent.Children
				.Where(x => x.DocumentTypeAlias.InvariantEquals(binder.Name) || x.DocumentTypeAlias.MakePluralName().InvariantEquals(binder.Name))
				.ToArray();
			if (filteredTypeChildren.Any())
			{
				return new Attempt<object>(true,
				                           new DynamicPublishedContentList(filteredTypeChildren.Select(x => new DynamicPublishedContentBase(x))));
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

			if (PublishedContent.DocumentTypeAlias == null && userProperty.Alias == null)
			{
				throw new InvalidOperationException("No node alias or property alias available. Unable to look up the datatype of the property you are trying to fetch.");
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
					TryGetCustomMember,		//match custom members
					TryGetUserProperty,		//then match custom user defined umbraco properties
					TryGetChildrenByAlias,	//then try to match children based on doc type alias
					TryGetDocumentProperty  //then try to match on a reflected document property
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

			//.Where explictly checks for this type
			//and will make it false
			//which means backwards equality (&& property != true) will pass
			//forwwards equality (&& property or && property == true) will fail
			result = new DynamicNull();

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
			return GetPropertyInternal(alias, PublishedContent, false);
		}

		/// <summary>
		/// Return a user defined property
		/// </summary>
		/// <param name="alias"></param>
		/// <param name="recursive"></param>
		/// <returns></returns>
		internal PropertyResult GetUserProperty(string alias, bool recursive = false)
		{
			if (!recursive)
			{
				return GetPropertyInternal(alias, PublishedContent);
			}
			var context = this;
			var prop = GetPropertyInternal(alias, PublishedContent);
			while (prop == null || !prop.HasValue())
			{
				var parent = ((IPublishedContent) context).Parent;
				if (parent == null) break;
				prop = context.GetPropertyInternal(alias, context.PublishedContent);
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
		
		public int TemplateId
		{
			get { return PublishedContent.TemplateId; }
		}

		public int SortOrder
		{
			get { return PublishedContent.SortOrder; }
		}

		public string Name
		{
			get { return PublishedContent.Name; }
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
			get { return PublishedContent.UrlName; }
		}

		public string DocumentTypeAlias
		{
			get { return PublishedContent.DocumentTypeAlias; }
		}

		public string WriterName
		{
			get { return PublishedContent.WriterName; }
		}

		public string CreatorName
		{
			get { return PublishedContent.CreatorName; }
		}

		public int WriterId
		{
			get { return PublishedContent.WriterId; }
		}

		public int CreatorId
		{
			get { return PublishedContent.CreatorId; }
		}

		public string Path
		{
			get { return PublishedContent.Path; }
		}

		public DateTime CreateDate
		{
			get { return PublishedContent.CreateDate; }
		}
		
		public int Id
		{
			get { return PublishedContent.Id; }
		}

		public DateTime UpdateDate
		{
			get { return PublishedContent.UpdateDate; }
		}

		public Guid Version
		{
			get { return PublishedContent.Version; }
		}
		
		public int Level
		{
			get { return PublishedContent.Level; }
		}

		public IEnumerable<IDocumentProperty> Properties
		{
			get { return PublishedContent.Properties; }
		}

		public DynamicPublishedContentList Children
		{
			get
			{
				if (_cachedChildren == null)
				{
					var children = PublishedContent.Children;
					//testing, think this must be a special case for the root node ?
					if (!children.Any() && PublishedContent.Id == 0)
					{
						_cachedChildren = new DynamicPublishedContentList(new List<DynamicPublishedContentBase> { new DynamicPublishedContentBase(this.PublishedContent) });
					}
					else
					{
						_cachedChildren = new DynamicPublishedContentList(PublishedContent.Children.Select(x => new DynamicPublishedContentBase(x)));
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
			var dynamicDocumentList = new DynamicPublishedContentList();
			dynamicDocumentList.Add(this);
			var filtered = dynamicDocumentList.Where<DynamicPublishedContentBase>(predicate);
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
			get { return PublishedContent.Parent; }
		}

		int IPublishedContent.Id
		{
			get { return PublishedContent.Id; }
		}

		int IPublishedContent.TemplateId
		{
			get { return PublishedContent.TemplateId; }
		}

		int IPublishedContent.SortOrder
		{
			get { return PublishedContent.SortOrder; }
		}

		string IPublishedContent.Name
		{
			get { return PublishedContent.Name; }
		}

		string IPublishedContent.UrlName
		{
			get { return PublishedContent.UrlName; }
		}

		string IPublishedContent.DocumentTypeAlias
		{
			get { return PublishedContent.DocumentTypeAlias; }
		}

		int IPublishedContent.DocumentTypeId
		{
			get { return PublishedContent.DocumentTypeId; }
		}

		string IPublishedContent.WriterName
		{
			get { return PublishedContent.WriterName; }
		}

		string IPublishedContent.CreatorName
		{
			get { return PublishedContent.CreatorName; }
		}

		int IPublishedContent.WriterId
		{
			get { return PublishedContent.WriterId; }
		}

		int IPublishedContent.CreatorId
		{
			get { return PublishedContent.CreatorId; }
		}

		string IPublishedContent.Path
		{
			get { return PublishedContent.Path; }
		}

		DateTime IPublishedContent.CreateDate
		{
			get { return PublishedContent.CreateDate; }
		}

		DateTime IPublishedContent.UpdateDate
		{
			get { return PublishedContent.UpdateDate; }
		}

		Guid IPublishedContent.Version
		{
			get { return PublishedContent.Version; }
		}

		int IPublishedContent.Level
		{
			get { return PublishedContent.Level; }
		}

		System.Collections.ObjectModel.Collection<IDocumentProperty> IPublishedContent.Properties
		{
			get { return PublishedContent.Properties; }
		}

		IEnumerable<IPublishedContent> IPublishedContent.Children
		{
			get { return PublishedContent.Children; }
		}

		IDocumentProperty IPublishedContent.GetProperty(string alias)
		{
			return PublishedContent.GetProperty(alias);
		} 
		#endregion
	}
}
