using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Web;
using Umbraco.Core.Configuration;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using System.Reflection;
using System.Xml.Linq;
using umbraco.cms.businesslogic;
using ContentType = umbraco.cms.businesslogic.ContentType;

namespace Umbraco.Web.Models
{

	/// <summary>
	/// The base dynamic model for views
	/// </summary>
    [DebuggerDisplay("Content Id: {Id}, Name: {Name}")]
    public class DynamicPublishedContent : DynamicObject, IPublishedContent, IOwnerCollectionAware<IPublishedContent>
	{
		protected internal IPublishedContent PublishedContent { get; private set; }
		private DynamicPublishedContentList _cachedChildren;
		private readonly ConcurrentDictionary<string, object> _cachedMemberOutput = new ConcurrentDictionary<string, object>();
		
		#region Constructors

		public DynamicPublishedContent(IPublishedContent content)
		{
			if (content == null) throw new ArgumentNullException("content");
			PublishedContent = content;
		}		
		
		#endregion

        private IEnumerable<IPublishedContent> _ownersCollection;

        /// <summary>
        /// Need to get/set the owner collection when an item is returned from the result set of a query
        /// </summary>
        /// <remarks>
        /// Based on this issue here: http://issues.umbraco.org/issue/U4-1797
        /// </remarks>
        IEnumerable<IPublishedContent> IOwnerCollectionAware<IPublishedContent>.OwnersCollection
        {
            get
            {
                var publishedContentBase = PublishedContent as IOwnerCollectionAware<IPublishedContent>;
                if (publishedContentBase != null)
                {
                    return publishedContentBase.OwnersCollection;
                }

                //if the owners collection is null, we'll default to it's siblings
                if (_ownersCollection == null)
                {
                    //get the root docs if parent is null
                    _ownersCollection = this.Siblings();
                }
                return _ownersCollection;
            }
            set
            {
                var publishedContentBase = PublishedContent as IOwnerCollectionAware<IPublishedContent>;
                if (publishedContentBase != null)
                {
                    publishedContentBase.OwnersCollection = value;
                }
                else
                {
                    _ownersCollection = value;    
                }
            }
        }       
        
		public dynamic AsDynamic()
		{
			return this;
		}

		public bool HasProperty(string name)
		{
			return PublishedContent.HasProperty(name);
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
			var attempt = DynamicInstanceHelper.TryInvokeMember(this, binder, args, new[]
        		{
					typeof(DynamicPublishedContent)
        		});

			if (attempt.Success)
			{
				result = attempt.Result.ObjectResult;

				//need to check the return type and possibly cast if result is from an extension method found
				if (attempt.Result.Reason == DynamicInstanceHelper.TryInvokeMemberSuccessReason.FoundExtensionMethod)
				{					
					//we don't need to cast if it is already DynamicPublishedContent
					if (attempt.Result.ObjectResult != null && (!(attempt.Result.ObjectResult is DynamicPublishedContent)))
					{
						if (attempt.Result.ObjectResult is IPublishedContent)
						{
							result = new DynamicPublishedContent((IPublishedContent)attempt.Result.ObjectResult);
						}
						else if (attempt.Result.ObjectResult is IEnumerable<DynamicPublishedContent>)
						{
							result = new DynamicPublishedContentList((IEnumerable<DynamicPublishedContent>)attempt.Result.ObjectResult);
						}	
						else if (attempt.Result.ObjectResult is IEnumerable<IPublishedContent>)
						{
							result = new DynamicPublishedContentList((IEnumerable<IPublishedContent>)attempt.Result.ObjectResult);
						}
					}	
				}
				return true;
			}
			
			//this is the result of an extension method execution gone wrong so we return dynamic null
			if (attempt.Result.Reason == DynamicInstanceHelper.TryInvokeMemberSuccessReason.FoundExtensionMethod
				&& attempt.Error != null && attempt.Error is TargetInvocationException) 				
			{
				result = new DynamicNull();
				return true;	
			}
		
			result = null;
			return false;
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
				                           new DynamicPublishedContentList(filteredTypeChildren.Select(x => new DynamicPublishedContent(x))));
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

			//get the data type id for the current property
			var dataType = Umbraco.Core.PublishedContentHelper.GetDataType(
                ApplicationContext.Current,
                userProperty.DocumentTypeAlias, 
                userProperty.Alias);

			//convert the string value to a known type
			var converted = Umbraco.Core.PublishedContentHelper.ConvertPropertyValue(result, dataType, userProperty.DocumentTypeAlias, userProperty.Alias);
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

                // Update the context before attempting to retrieve the property again.
                context = parent.AsDynamicPublishedContent();
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

		#region Standard Properties
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
			get { return PublishedContent.IsVisible(); }
		}

		public bool IsVisible()
		{
			return PublishedContent.IsVisible();
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

		public string Url
		{
			get { return PublishedContent.Url; }
		}

		public PublishedItemType ItemType
		{
			get { return PublishedContent.ItemType; }
		}

		public IEnumerable<IPublishedContentProperty> Properties
		{
			get { return PublishedContent.Properties; }
		}

		public object this[string propertyAlias]
		{
			get { return PublishedContent[propertyAlias]; }
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
						_cachedChildren = new DynamicPublishedContentList(new List<DynamicPublishedContent> { new DynamicPublishedContent(this.PublishedContent) });
					}
					else
					{
						_cachedChildren = new DynamicPublishedContentList(PublishedContent.Children.Select(x => new DynamicPublishedContent(x)));
					}
				}
				return _cachedChildren;
			}
		} 
		#endregion

		public string GetTemplateAlias()
		{
			return PublishedContentExtensions.GetTemplateAlias(this);
		}

		#region Search

		public DynamicPublishedContentList Search(string term, bool useWildCards = true, string searchProvider = null)
		{
			return new DynamicPublishedContentList(
				PublishedContentExtensions.Search(this, term, useWildCards, searchProvider));
		}

		public DynamicPublishedContentList SearchDescendants(string term, bool useWildCards = true, string searchProvider = null)
		{
			return new DynamicPublishedContentList(
				PublishedContentExtensions.SearchDescendants(this, term, useWildCards, searchProvider));
		}

		public DynamicPublishedContentList SearchChildren(string term, bool useWildCards = true, string searchProvider = null)
		{
			return new DynamicPublishedContentList(
				PublishedContentExtensions.SearchChildren(this, term, useWildCards, searchProvider));
		}

		public DynamicPublishedContentList Search(Examine.SearchCriteria.ISearchCriteria criteria, Examine.Providers.BaseSearchProvider searchProvider = null)
		{
			return new DynamicPublishedContentList(
				PublishedContentExtensions.Search(this, criteria, searchProvider));
		}

		#endregion

		#region GetProperty methods which can be used with the dynamic object

		public IPublishedContentProperty GetProperty(string alias)
		{
            var prop = GetProperty(alias, false);
			if (prop == null && alias.StartsWith("_"))
			{
			    //if it is prefixed and the first result failed, try to get it by recursive
                var recursiveAlias = alias.Substring(1, alias.Length - 1);
                return GetProperty(recursiveAlias, true);
			}
		    return prop;
		}
		public IPublishedContentProperty GetProperty(string alias, bool recursive)
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
			return this.PublishedContent.HasValue(alias);
		}
		public bool HasValue(string alias, bool recursive)
		{
			return this.PublishedContent.HasValue(alias, recursive);
		}
		public IHtmlString HasValue(string alias, string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.HasValue(alias, valueIfTrue, valueIfFalse);
		}
		public IHtmlString HasValue(string alias, bool recursive, string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.HasValue(alias, recursive, valueIfTrue, valueIfFalse);
		}
		public IHtmlString HasValue(string alias, string valueIfTrue)
		{
			return this.PublishedContent.HasValue(alias, valueIfTrue);
		}
		public IHtmlString HasValue(string alias, bool recursive, string valueIfTrue)
		{
			return this.PublishedContent.HasValue(alias, recursive, valueIfTrue);
		} 
		#endregion

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

		ICollection<IPublishedContentProperty> IPublishedContent.Properties
		{
			get { return PublishedContent.Properties; }
		}

		IEnumerable<IPublishedContent> IPublishedContent.Children
		{
			get { return PublishedContent.Children; }
		}

		IPublishedContentProperty IPublishedContent.GetProperty(string alias)
		{
			return PublishedContent.GetProperty(alias);
		} 
		#endregion

		
		#region Index/Position
		public int Position()
		{
			return Umbraco.Web.PublishedContentExtensions.Position(this);
		}
		public int Index()
		{
			return Umbraco.Web.PublishedContentExtensions.Index(this);
		} 
		#endregion

		#region Is Helpers

		public bool IsDocumentType(string docTypeAlias)
		{
			return this.PublishedContent.IsDocumentType(docTypeAlias);
		}

		public bool IsNull(string alias, bool recursive)
		{
			return this.PublishedContent.IsNull(alias, recursive);
		}
		public bool IsNull(string alias)
		{
			return this.PublishedContent.IsNull(alias, false);
		}
		public bool IsFirst()
		{
			return this.PublishedContent.IsFirst();
		}
		public HtmlString IsFirst(string valueIfTrue)
		{
			return this.PublishedContent.IsFirst(valueIfTrue);
		}
		public HtmlString IsFirst(string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsFirst(valueIfTrue, valueIfFalse);
		}
		public bool IsNotFirst()
		{
			return this.PublishedContent.IsNotFirst();
		}
		public HtmlString IsNotFirst(string valueIfTrue)
		{
			return this.PublishedContent.IsNotFirst(valueIfTrue);
		}
		public HtmlString IsNotFirst(string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsNotFirst(valueIfTrue, valueIfFalse);
		}
		public bool IsPosition(int index)
		{
			return this.PublishedContent.IsPosition(index);
		}
		public HtmlString IsPosition(int index, string valueIfTrue)
		{
			return this.PublishedContent.IsPosition(index, valueIfTrue);
		}
		public HtmlString IsPosition(int index, string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsPosition(index, valueIfTrue, valueIfFalse);
		}
		public bool IsModZero(int modulus)
		{
			return this.PublishedContent.IsModZero(modulus);
		}
		public HtmlString IsModZero(int modulus, string valueIfTrue)
		{
			return this.PublishedContent.IsModZero(modulus, valueIfTrue);
		}
		public HtmlString IsModZero(int modulus, string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsModZero(modulus, valueIfTrue, valueIfFalse);
		}

		public bool IsNotModZero(int modulus)
		{
			return this.PublishedContent.IsNotModZero(modulus);
		}
		public HtmlString IsNotModZero(int modulus, string valueIfTrue)
		{
			return this.PublishedContent.IsNotModZero(modulus, valueIfTrue);
		}
		public HtmlString IsNotModZero(int modulus, string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsNotModZero(modulus, valueIfTrue, valueIfFalse);
		}
		public bool IsNotPosition(int index)
		{
			return this.PublishedContent.IsNotPosition(index);
		}
		public HtmlString IsNotPosition(int index, string valueIfTrue)
		{
			return this.PublishedContent.IsNotPosition(index, valueIfTrue);
		}
		public HtmlString IsNotPosition(int index, string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsNotPosition(index, valueIfTrue, valueIfFalse);
		}
		public bool IsLast()
		{
			return this.PublishedContent.IsLast();
		}
		public HtmlString IsLast(string valueIfTrue)
		{
			return this.PublishedContent.IsLast(valueIfTrue);
		}
		public HtmlString IsLast(string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsLast(valueIfTrue, valueIfFalse);
		}
		public bool IsNotLast()
		{
			return this.PublishedContent.IsNotLast();
		}
		public HtmlString IsNotLast(string valueIfTrue)
		{
			return this.PublishedContent.IsNotLast(valueIfTrue);
		}
		public HtmlString IsNotLast(string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsNotLast(valueIfTrue, valueIfFalse);
		}
		public bool IsEven()
		{
			return this.PublishedContent.IsEven();
		}
		public HtmlString IsEven(string valueIfTrue)
		{
			return this.PublishedContent.IsEven(valueIfTrue);
		}
		public HtmlString IsEven(string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsEven(valueIfTrue, valueIfFalse);
		}
		public bool IsOdd()
		{
			return this.PublishedContent.IsOdd();
		}
		public HtmlString IsOdd(string valueIfTrue)
		{
			return this.PublishedContent.IsOdd(valueIfTrue);
		}
		public HtmlString IsOdd(string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsOdd(valueIfTrue, valueIfFalse);
		}
		public bool IsEqual(DynamicPublishedContent other)
		{
			return this.PublishedContent.IsEqual(other);
		}
		public HtmlString IsEqual(DynamicPublishedContent other, string valueIfTrue)
		{
			return this.PublishedContent.IsEqual(other, valueIfTrue);
		}
		public HtmlString IsEqual(DynamicPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsEqual(other, valueIfTrue, valueIfFalse);
		}
		public bool IsNotEqual(DynamicPublishedContent other)
		{
			return this.PublishedContent.IsNotEqual(other);
		}
		public HtmlString IsNotEqual(DynamicPublishedContent other, string valueIfTrue)
		{
			return this.PublishedContent.IsNotEqual(other, valueIfTrue);
		}
		public HtmlString IsNotEqual(DynamicPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsNotEqual(other, valueIfTrue, valueIfFalse);
		}
		public bool IsDescendant(DynamicPublishedContent other)
		{
			return this.PublishedContent.IsDescendant(other);
		}
		public HtmlString IsDescendant(DynamicPublishedContent other, string valueIfTrue)
		{
			return this.PublishedContent.IsDescendant(other, valueIfTrue);
		}
		public HtmlString IsDescendant(DynamicPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsDescendant(other, valueIfTrue, valueIfFalse);
		}
		public bool IsDescendantOrSelf(DynamicPublishedContent other)
		{
			return this.PublishedContent.IsDescendantOrSelf(other);
		}
		public HtmlString IsDescendantOrSelf(DynamicPublishedContent other, string valueIfTrue)
		{
			return this.PublishedContent.IsDescendantOrSelf(other, valueIfTrue);
		}
		public HtmlString IsDescendantOrSelf(DynamicPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsDescendantOrSelf(other, valueIfTrue, valueIfFalse);
		}
		public bool IsAncestor(DynamicPublishedContent other)
		{
			return this.PublishedContent.IsAncestor(other);
		}
		public HtmlString IsAncestor(DynamicPublishedContent other, string valueIfTrue)
		{
			return this.PublishedContent.IsAncestor(other, valueIfTrue);
		}
		public HtmlString IsAncestor(DynamicPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsAncestor(other, valueIfTrue, valueIfFalse);
		}
		public bool IsAncestorOrSelf(DynamicPublishedContent other)
		{
			return this.PublishedContent.IsAncestorOrSelf(other);
		}
		public HtmlString IsAncestorOrSelf(DynamicPublishedContent other, string valueIfTrue)
		{
			return this.PublishedContent.IsAncestorOrSelf(other, valueIfTrue);
		}
		public HtmlString IsAncestorOrSelf(DynamicPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsAncestorOrSelf(other, valueIfTrue, valueIfFalse);
		}		
		#endregion

		#region Traversal
		public DynamicPublishedContent Up()
		{
			return Umbraco.Web.PublishedContentExtensions.Up(this).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent Up(int number)
		{
			return Umbraco.Web.PublishedContentExtensions.Up(this, number).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent Up(string nodeTypeAlias)
		{
			return Umbraco.Web.PublishedContentExtensions.Up(this, nodeTypeAlias).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent Down()
		{
			return Umbraco.Web.PublishedContentExtensions.Down(this).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent Down(int number)
		{
			return Umbraco.Web.PublishedContentExtensions.Down(this, number).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent Down(string nodeTypeAlias)
		{
			return Umbraco.Web.PublishedContentExtensions.Down(this, nodeTypeAlias).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent Next()
		{
			return Umbraco.Web.PublishedContentExtensions.Next(this).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent Next(int number)
		{
			return Umbraco.Web.PublishedContentExtensions.Next(this, number).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent Next(string nodeTypeAlias)
		{
			return Umbraco.Web.PublishedContentExtensions.Next(this, nodeTypeAlias).AsDynamicPublishedContent();
		}

		public DynamicPublishedContent Previous()
		{
			return Umbraco.Web.PublishedContentExtensions.Previous(this).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent Previous(int number)
		{
			return Umbraco.Web.PublishedContentExtensions.Previous(this, number).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent Previous(string nodeTypeAlias)
		{
			return Umbraco.Web.PublishedContentExtensions.Previous(this, nodeTypeAlias).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent Sibling(int number)
		{
			return Umbraco.Web.PublishedContentExtensions.Previous(this, number).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent Sibling(string nodeTypeAlias)
		{
			return Umbraco.Web.PublishedContentExtensions.Previous(this, nodeTypeAlias).AsDynamicPublishedContent();
		} 
		#endregion

		#region Ancestors, Descendants and Parent
		#region Ancestors
		public DynamicPublishedContentList Ancestors(int level)
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.Ancestors(this, level));
		}
		public DynamicPublishedContentList Ancestors(string nodeTypeAlias)
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.Ancestors(this, nodeTypeAlias));
		}
		public DynamicPublishedContentList Ancestors()
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.Ancestors(this));
		}
		public DynamicPublishedContentList Ancestors(Func<IPublishedContent, bool> func)
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.Ancestors(this, func));
		}
		public DynamicPublishedContent AncestorOrSelf()
		{
			return Umbraco.Web.PublishedContentExtensions.AncestorOrSelf(this).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent AncestorOrSelf(int level)
		{
			return Umbraco.Web.PublishedContentExtensions.AncestorOrSelf(this, level).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent AncestorOrSelf(string nodeTypeAlias)
		{
			return Umbraco.Web.PublishedContentExtensions.AncestorOrSelf(this, nodeTypeAlias).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent AncestorOrSelf(Func<IPublishedContent, bool> func)
		{
			return Umbraco.Web.PublishedContentExtensions.AncestorOrSelf(this, func).AsDynamicPublishedContent();
		}
		public DynamicPublishedContentList AncestorsOrSelf(Func<IPublishedContent, bool> func)
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.AncestorsOrSelf(this, func));
		}
		public DynamicPublishedContentList AncestorsOrSelf()
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.AncestorsOrSelf(this));
		}
		public DynamicPublishedContentList AncestorsOrSelf(string nodeTypeAlias)
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.AncestorsOrSelf(this, nodeTypeAlias));
		}
		public DynamicPublishedContentList AncestorsOrSelf(int level)
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.AncestorsOrSelf(this, level));
		} 
		#endregion
		#region Descendants
		public DynamicPublishedContentList Descendants(string nodeTypeAlias)
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.Descendants(this, nodeTypeAlias));
		}
		public DynamicPublishedContentList Descendants(int level)
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.Descendants(this, level));
		}
		public DynamicPublishedContentList Descendants()
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.Descendants(this));
		}
		public DynamicPublishedContentList DescendantsOrSelf(int level)
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.DescendantsOrSelf(this, level));
		}
		public DynamicPublishedContentList DescendantsOrSelf(string nodeTypeAlias)
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.DescendantsOrSelf(this, nodeTypeAlias));
		}
		public DynamicPublishedContentList DescendantsOrSelf()
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.DescendantsOrSelf(this));
		} 
		#endregion

		public DynamicPublishedContent Parent
		{
			get
			{
				if (PublishedContent.Parent != null)
				{
					return PublishedContent.Parent.AsDynamicPublishedContent();
				}
				if (PublishedContent != null && PublishedContent.Id == 0)
				{
					return this;
				}
				return null;
			}
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
			var filtered = dynamicDocumentList.Where<DynamicPublishedContent>(predicate);
			if (Queryable.Count(filtered) == 1)
			{
				//this node matches the predicate
				return true;
			}
			return false;
		}

		#endregion
		
	}
}
