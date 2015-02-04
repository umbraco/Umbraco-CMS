// ENABLE THE FIX in 7.0.0
// TODO if all goes well, remove the obsolete code eventually
#define FIX_GET_PROPERTY_VALUE

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Web;
using Umbraco.Core.Cache;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;
using Umbraco.Core;
using System.Reflection;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Models
{

	/// <summary>
	/// The base dynamic model for views
	/// </summary>
    [DebuggerDisplay("Content Id: {Id}, Name: {Name}")]
    public class DynamicPublishedContent : DynamicObject, IPublishedContent
	{
		protected internal IPublishedContent PublishedContent { get; private set; }
	    private DynamicPublishedContentList _contentList;

        // must implement that one if we implement IPublishedContent
	    public IEnumerable<IPublishedContent> ContentSet
	    {
            // that is a definitively non-efficient way of doing it, though it should work
	        get { return _contentList ?? (_contentList = new DynamicPublishedContentList(PublishedContent.ContentSet)); }
	    }

        public PublishedContentType ContentType { get { return PublishedContent.ContentType; } }

		#region Constructors

		public DynamicPublishedContent(IPublishedContent content)
		{
			if (content == null) throw new ArgumentNullException("content");
			PublishedContent = content;
		}

        internal DynamicPublishedContent(IPublishedContent content, DynamicPublishedContentList contentList)
        {
            PublishedContent = content;
            _contentList = contentList;
        }
		
		#endregion

        // these two here have leaked in v6 and so we cannot remove them anymore
        // without breaking compatibility but... TODO: remove them in v7

        [Obsolete("Will be removing in future versions")]
        public DynamicPublishedContentList ChildrenAsList { get { return Children; } }
        
        [Obsolete("Will be removing in future versions")]
        public int parentId { get { return PublishedContent.Parent.Id; } }

        #region DynamicObject

        private readonly ConcurrentDictionary<string, object> _cachedMemberOutput = new ConcurrentDictionary<string, object>();

        /// <summary>
		/// Attempts to call a method on the dynamic object
		/// </summary>
		/// <param name="binder"></param>
		/// <param name="args"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
            var runtimeCache = ApplicationContext.Current != null ? ApplicationContext.Current.ApplicationCache.RuntimeCache : new NullCacheProvider();

            var attempt = DynamicInstanceHelper.TryInvokeMember(runtimeCache, this, binder, args, new[]
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
			if (attempt.Result != null
                && attempt.Result.Reason == DynamicInstanceHelper.TryInvokeMemberSuccessReason.FoundExtensionMethod
				&& attempt.Exception is TargetInvocationException) 				
			{
			    result = DynamicNull.Null;
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
            // as of 4.5 the CLR is case-sensitive which means that the default binder
            // will handle those methods only when using the proper casing. So what
            // this method does is ensure that any casing is supported.

			if (binder.Name.InvariantEquals("ChildrenAsList") || binder.Name.InvariantEquals("Children"))
			{
				return Attempt<object>.Succeed(Children);
			}

			if (binder.Name.InvariantEquals("parentId"))
			{
				var parent = ((IPublishedContent) this).Parent;
				if (parent == null)
				{
					throw new InvalidOperationException(string.Format("The node {0} does not have a parent", Id));
				}
				return Attempt<object>.Succeed(parent.Id);
			}

			return Attempt<object>.Fail();
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
				return Attempt<object>.Succeed(
				                           new DynamicPublishedContentList(filteredTypeChildren.Select(x => new DynamicPublishedContent(x))));
			}
			return Attempt<object>.Fail();
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

		    return Attempt.If(result != null, result);
		}

		/// <summary>
		/// Attempts to return a member based on a user defined umbraco property
		/// </summary>
		/// <param name="binder"></param>
		/// <returns></returns>
		protected virtual Attempt<object> TryGetUserProperty(GetMemberBinder binder)
		{
			var name = binder.Name;
			var recurse = false;
			if (name.StartsWith("_"))
			{
				name = name.Substring(1, name.Length - 1);
				recurse = true;
			}

		    var value = PublishedContent.GetPropertyValue(name, recurse);
			return Attempt<object>.SucceedIf(value != null, value);
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
            result = DynamicNull.Null;

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

			while (prop == null || !prop.HasValue)
			{
				var parent = ((IPublishedContent) context).Parent;
				if (parent == null) break;

                // Update the context before attempting to retrieve the property again.
                context = parent.AsDynamicOrNull();
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

                // get wrap the result in a PropertyResult - just so it's an IHtmlString - ?!
                return prop == null
			        ? null
			        : new PropertyResult(prop, PropertyResultType.UserProperty);
			}

			//reflect

            // as of 4.5 the CLR is case-sensitive which means that the default binder
            // can handle properties only when using the proper casing. So what this
            // does is ensure that any casing is supported.

            var attempt = content.GetType().GetMemberIgnoreCase(content, alias);

			return attempt.Success == false || attempt.Result == null
				? null
				: new PropertyResult(alias, attempt.Result, PropertyResultType.ReflectedProperty);
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

        bool IPublishedContent.IsDraft
	    {
            get { return PublishedContent.IsDraft; }
	    }

        int IPublishedContent.GetIndex()
        {
            return PublishedContent.GetIndex();
        }

        ICollection<IPublishedProperty> IPublishedContent.Properties
        {
            get { return PublishedContent.Properties; }
        }

        IEnumerable<IPublishedContent> IPublishedContent.Children
        {
            get { return PublishedContent.Children; }
        }

        IPublishedProperty IPublishedContent.GetProperty(string alias)
        {
            return PublishedContent.GetProperty(alias);
        }

        #endregion

        #region IPublishedContent implementation

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

        // see note in IPublishedContent
        //public bool Published
        //{
        //    get { return PublishedContent.Published; }
        //}

		public IEnumerable<IPublishedProperty> Properties
		{
			get { return PublishedContent.Properties; }
		}

		public object this[string propertyAlias]
		{
			get { return PublishedContent[propertyAlias]; }
		}

        #endregion

        #region GetProperty

        // enhanced versions of the extension methods that exist for IPublishedContent,
        // here we support the recursive (_) and reflected (@) syntax

        public IPublishedProperty GetProperty(string alias)
        {
            return alias.StartsWith("_")
                ? GetProperty(alias.Substring(1), true)
                : GetProperty(alias, false);
        }

        public IPublishedProperty GetProperty(string alias, bool recurse)
        {
            if (alias.StartsWith("@")) return GetReflectedProperty(alias.Substring(1));

            // get wrap the result in a PropertyResult - just so it's an IHtmlString - ?!
            var property = PublishedContent.GetProperty(alias, recurse);
            return property == null ? null : new PropertyResult(property, PropertyResultType.UserProperty);
        }

        #endregion

        // IPublishedContent extension methods:
        //
        // all these methods are IPublishedContent extension methods so they should in
        // theory apply to DynamicPublishedContent since it is an IPublishedContent and
        // we look for extension methods. But that lookup has to be pretty slow.
        // Duplicating the methods here makes things much faster.

        #region IPublishedContent extension methods - Template

        public string GetTemplateAlias()
		{
			return PublishedContentExtensions.GetTemplateAlias(this);
		}
        
        #endregion

        #region IPublishedContent extension methods - HasProperty

        public bool HasProperty(string name)
        {
            return PublishedContent.HasProperty(name);
        }
        
        #endregion

        #region IPublishedContent extension methods - HasValue

        public bool HasValue(string alias)
		{
			return PublishedContent.HasValue(alias);
		}

		public bool HasValue(string alias, bool recursive)
		{
			return PublishedContent.HasValue(alias, recursive);
		}

		public IHtmlString HasValue(string alias, string valueIfTrue, string valueIfFalse)
		{
			return PublishedContent.HasValue(alias, valueIfTrue, valueIfFalse);
		}

		public IHtmlString HasValue(string alias, bool recursive, string valueIfTrue, string valueIfFalse)
		{
			return PublishedContent.HasValue(alias, recursive, valueIfTrue, valueIfFalse);
		}

		public IHtmlString HasValue(string alias, string valueIfTrue)
		{
			return PublishedContent.HasValue(alias, valueIfTrue);
		}

		public IHtmlString HasValue(string alias, bool recursive, string valueIfTrue)
		{
			return PublishedContent.HasValue(alias, recursive, valueIfTrue);
		} 

		#endregion

        #region IPublishedContent extension methods - GetPropertyValue

        // for whatever reason, some methods returning strings were created in DynamicPublishedContent
        // and are now considered a "feature" as of v6. So we can't have the proper GetPropertyValue
        // methods returning objects, too. And we don't want to change it in v6 as that would be a 
        // breaking change.

#if FIX_GET_PROPERTY_VALUE

        public object GetPropertyValue(string alias)
        {
            return PublishedContent.GetPropertyValue(alias);
        }

        public object GetPropertyValue(string alias, string defaultValue)
        {
            return PublishedContent.GetPropertyValue(alias, defaultValue);
        }

        public object GetPropertyValue(string alias, object defaultValue)
        {
            return PublishedContent.GetPropertyValue(alias, defaultValue);
        }

        public object GetPropertyValue(string alias, bool recurse)
        {
            return PublishedContent.GetPropertyValue(alias, recurse);
        }

        public object GetPropertyValue(string alias, bool recurse, object defaultValue)
        {
            return PublishedContent.GetPropertyValue(alias, recurse, defaultValue);
        }

#else

        public string GetPropertyValue(string alias)
        {
            return GetPropertyValue(alias, false);
        }

        public string GetPropertyValue(string alias, string defaultValue)
        {
            var value = GetPropertyValue(alias);
            return value.IsNullOrWhiteSpace() ? defaultValue : value;
        }

        public string GetPropertyValue(string alias, bool recurse, string defaultValue)
        {
            var value = GetPropertyValue(alias, recurse);
            return value.IsNullOrWhiteSpace() ? defaultValue : value;
        }

        public string GetPropertyValue(string alias, bool recursive)
        {
            var property = GetProperty(alias, recursive);
            if (property == null || property.Value == null) return null;
            return property.Value.ToString();
        }

#endif

        #endregion

        #region IPublishedContent extension methods - GetPropertyValue<T>

        public T GetPropertyValue<T>(string alias)
        {
            return PublishedContent.GetPropertyValue<T>(alias);
        }

        public T GetPropertyValue<T>(string alias, T defaultValue)
        {
            return PublishedContent.GetPropertyValue(alias, defaultValue);
        }

        public T GetPropertyValue<T>(string alias, bool recurse)
        {
            return PublishedContent.GetPropertyValue<T>(alias, recurse);
        }

        public T GetPropertyValue<T>(string alias, bool recurse, T defaultValue)
        {
            return PublishedContent.GetPropertyValue(alias, recurse, defaultValue);
        }

        #endregion

        #region IPublishedContent extension methods - Search

        public DynamicPublishedContentList Search(string term, bool useWildCards = true, string searchProvider = null)
        {
            return new DynamicPublishedContentList(PublishedContent.Search(term, useWildCards, searchProvider));
        }

        public DynamicPublishedContentList SearchDescendants(string term, bool useWildCards = true, string searchProvider = null)
        {
            return new DynamicPublishedContentList(PublishedContent.SearchDescendants(term, useWildCards, searchProvider));
        }

        public DynamicPublishedContentList SearchChildren(string term, bool useWildCards = true, string searchProvider = null)
        {
            return new DynamicPublishedContentList(PublishedContent.SearchChildren(term, useWildCards, searchProvider));
        }

        public DynamicPublishedContentList Search(Examine.SearchCriteria.ISearchCriteria criteria, Examine.Providers.BaseSearchProvider searchProvider = null)
        {
            return new DynamicPublishedContentList(PublishedContent.Search(criteria, searchProvider));
        }

        #endregion

        #region IPublishedContent extension methods - AsDynamic

        public dynamic AsDynamic()
        {
            return this;
        }

        public dynamic AsDynamicOrNull()
        {
            return this;
        }
        
        #endregion

        #region IPublishedContente extension methods - ContentSet

        public int Position()
        {
            return Index();
        }

        public int Index()
        {
            return PublishedContent.GetIndex();
        }

        #endregion

        #region IPublishedContent extension methods - IsSomething: misc

        public bool Visible
	    {
	        get { return PublishedContent.IsVisible(); }
	    }

        public bool IsVisible()
        {
            return PublishedContent.IsVisible();
        }

		public bool IsDocumentType(string docTypeAlias)
		{
			return PublishedContent.IsDocumentType(docTypeAlias);
		}

		public bool IsNull(string alias, bool recursive)
		{
			return PublishedContent.IsNull(alias, recursive);
		}

		public bool IsNull(string alias)
		{
			return PublishedContent.IsNull(alias, false);
		}

        #endregion 

        #region IPublishedContent extension methods - IsSomething: position in set

        public bool IsFirst()
		{
			return PublishedContent.IsFirst();
		}

		public HtmlString IsFirst(string valueIfTrue)
		{
			return PublishedContent.IsFirst(valueIfTrue);
		}

		public HtmlString IsFirst(string valueIfTrue, string valueIfFalse)
		{
			return PublishedContent.IsFirst(valueIfTrue, valueIfFalse);
		}

		public bool IsNotFirst()
		{
			return PublishedContent.IsNotFirst();
		}

		public HtmlString IsNotFirst(string valueIfTrue)
		{
			return PublishedContent.IsNotFirst(valueIfTrue);
		}

		public HtmlString IsNotFirst(string valueIfTrue, string valueIfFalse)
		{
			return PublishedContent.IsNotFirst(valueIfTrue, valueIfFalse);
		}

		public bool IsPosition(int index)
		{
			return PublishedContent.IsPosition(index);
		}

		public HtmlString IsPosition(int index, string valueIfTrue)
		{
			return PublishedContent.IsPosition(index, valueIfTrue);
		}

		public HtmlString IsPosition(int index, string valueIfTrue, string valueIfFalse)
		{
			return PublishedContent.IsPosition(index, valueIfTrue, valueIfFalse);
		}

		public bool IsModZero(int modulus)
		{
			return PublishedContent.IsModZero(modulus);
		}

		public HtmlString IsModZero(int modulus, string valueIfTrue)
		{
			return PublishedContent.IsModZero(modulus, valueIfTrue);
		}

		public HtmlString IsModZero(int modulus, string valueIfTrue, string valueIfFalse)
		{
			return PublishedContent.IsModZero(modulus, valueIfTrue, valueIfFalse);
		}

		public bool IsNotModZero(int modulus)
		{
			return PublishedContent.IsNotModZero(modulus);
		}

		public HtmlString IsNotModZero(int modulus, string valueIfTrue)
		{
			return PublishedContent.IsNotModZero(modulus, valueIfTrue);
		}

		public HtmlString IsNotModZero(int modulus, string valueIfTrue, string valueIfFalse)
		{
			return PublishedContent.IsNotModZero(modulus, valueIfTrue, valueIfFalse);
		}

		public bool IsNotPosition(int index)
		{
			return PublishedContent.IsNotPosition(index);
		}

		public HtmlString IsNotPosition(int index, string valueIfTrue)
		{
			return PublishedContent.IsNotPosition(index, valueIfTrue);
		}

		public HtmlString IsNotPosition(int index, string valueIfTrue, string valueIfFalse)
		{
			return PublishedContent.IsNotPosition(index, valueIfTrue, valueIfFalse);
		}

		public bool IsLast()
		{
			return PublishedContent.IsLast();
		}

		public HtmlString IsLast(string valueIfTrue)
		{
			return PublishedContent.IsLast(valueIfTrue);
		}

		public HtmlString IsLast(string valueIfTrue, string valueIfFalse)
		{
			return PublishedContent.IsLast(valueIfTrue, valueIfFalse);
		}

		public bool IsNotLast()
		{
			return PublishedContent.IsNotLast();
		}

		public HtmlString IsNotLast(string valueIfTrue)
		{
			return PublishedContent.IsNotLast(valueIfTrue);
		}

		public HtmlString IsNotLast(string valueIfTrue, string valueIfFalse)
		{
			return PublishedContent.IsNotLast(valueIfTrue, valueIfFalse);
		}

		public bool IsEven()
		{
			return PublishedContent.IsEven();
		}

		public HtmlString IsEven(string valueIfTrue)
		{
			return PublishedContent.IsEven(valueIfTrue);
		}

		public HtmlString IsEven(string valueIfTrue, string valueIfFalse)
		{
			return PublishedContent.IsEven(valueIfTrue, valueIfFalse);
		}

		public bool IsOdd()
		{
			return PublishedContent.IsOdd();
		}

		public HtmlString IsOdd(string valueIfTrue)
		{
			return PublishedContent.IsOdd(valueIfTrue);
		}

		public HtmlString IsOdd(string valueIfTrue, string valueIfFalse)
		{
			return PublishedContent.IsOdd(valueIfTrue, valueIfFalse);
		}

        #endregion

        #region IPublishedContent extension methods - IsSomething: equality

        public bool IsEqual(DynamicPublishedContent other)
		{
			return PublishedContent.IsEqual(other);
		}

		public HtmlString IsEqual(DynamicPublishedContent other, string valueIfTrue)
		{
			return PublishedContent.IsEqual(other, valueIfTrue);
		}

		public HtmlString IsEqual(DynamicPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
			return PublishedContent.IsEqual(other, valueIfTrue, valueIfFalse);
		}

		public bool IsNotEqual(DynamicPublishedContent other)
		{
			return PublishedContent.IsNotEqual(other);
		}

		public HtmlString IsNotEqual(DynamicPublishedContent other, string valueIfTrue)
		{
			return PublishedContent.IsNotEqual(other, valueIfTrue);
		}

		public HtmlString IsNotEqual(DynamicPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
			return PublishedContent.IsNotEqual(other, valueIfTrue, valueIfFalse);
		}

        #endregion

        #region IPublishedContent extension methods - IsSomething: ancestors and descendants

        public bool IsDescendant(DynamicPublishedContent other)
		{
			return PublishedContent.IsDescendant(other);
		}

		public HtmlString IsDescendant(DynamicPublishedContent other, string valueIfTrue)
		{
			return PublishedContent.IsDescendant(other, valueIfTrue);
		}

		public HtmlString IsDescendant(DynamicPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
			return PublishedContent.IsDescendant(other, valueIfTrue, valueIfFalse);
		}

		public bool IsDescendantOrSelf(DynamicPublishedContent other)
		{
			return PublishedContent.IsDescendantOrSelf(other);
		}

		public HtmlString IsDescendantOrSelf(DynamicPublishedContent other, string valueIfTrue)
		{
			return PublishedContent.IsDescendantOrSelf(other, valueIfTrue);
		}

		public HtmlString IsDescendantOrSelf(DynamicPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
			return PublishedContent.IsDescendantOrSelf(other, valueIfTrue, valueIfFalse);
		}

		public bool IsAncestor(DynamicPublishedContent other)
		{
			return PublishedContent.IsAncestor(other);
		}

		public HtmlString IsAncestor(DynamicPublishedContent other, string valueIfTrue)
		{
			return PublishedContent.IsAncestor(other, valueIfTrue);
		}

		public HtmlString IsAncestor(DynamicPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
			return PublishedContent.IsAncestor(other, valueIfTrue, valueIfFalse);
		}

		public bool IsAncestorOrSelf(DynamicPublishedContent other)
		{
			return PublishedContent.IsAncestorOrSelf(other);
		}

		public HtmlString IsAncestorOrSelf(DynamicPublishedContent other, string valueIfTrue)
		{
			return PublishedContent.IsAncestorOrSelf(other, valueIfTrue);
		}

		public HtmlString IsAncestorOrSelf(DynamicPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
			return PublishedContent.IsAncestorOrSelf(other, valueIfTrue, valueIfFalse);
		}

		#endregion

        // all these methods wrap whatever PublishedContent returns in a new
        // DynamicPublishedContentList, for dynamic usage.

        #region Ancestors

        public DynamicPublishedContentList Ancestors(int level)
        {
            return new DynamicPublishedContentList(PublishedContent.Ancestors(level));
        }

        public DynamicPublishedContentList Ancestors(string contentTypeAlias)
        {
            return new DynamicPublishedContentList(PublishedContent.Ancestors(contentTypeAlias));
        }

        public DynamicPublishedContentList Ancestors()
        {
            return new DynamicPublishedContentList(PublishedContent.Ancestors());
        }

        public DynamicPublishedContentList Ancestors(Func<IPublishedContent, bool> func)
        {
            return new DynamicPublishedContentList(PublishedContent.AncestorsOrSelf(false, func));
        }

        public DynamicPublishedContent AncestorOrSelf()
        {
            return PublishedContent.AncestorOrSelf().AsDynamicOrNull();
        }

        public DynamicPublishedContent AncestorOrSelf(int level)
        {
            return PublishedContent.AncestorOrSelf(level).AsDynamicOrNull();
        }

        /// <summary>
        /// A shortcut method for AncestorOrSelf(1)
        /// </summary>
        /// <returns>
        /// The site homepage
        /// </returns>
	    public DynamicPublishedContent Site()
	    {
            return AncestorOrSelf(1);
	    }

        public DynamicPublishedContent AncestorOrSelf(string contentTypeAlias)
        {
            return PublishedContent.AncestorOrSelf(contentTypeAlias).AsDynamicOrNull();
        }

        public DynamicPublishedContent AncestorOrSelf(Func<IPublishedContent, bool> func)
        {
            return PublishedContent.AncestorsOrSelf(true, func).FirstOrDefault().AsDynamicOrNull();
        }

        public DynamicPublishedContentList AncestorsOrSelf(Func<IPublishedContent, bool> func)
        {
            return new DynamicPublishedContentList(PublishedContent.AncestorsOrSelf(true, func));
        }

        public DynamicPublishedContentList AncestorsOrSelf()
        {
            return new DynamicPublishedContentList(PublishedContent.AncestorsOrSelf());
        }

        public DynamicPublishedContentList AncestorsOrSelf(string contentTypeAlias)
        {
            return new DynamicPublishedContentList(PublishedContent.AncestorsOrSelf(contentTypeAlias));
        }

        public DynamicPublishedContentList AncestorsOrSelf(int level)
        {
            return new DynamicPublishedContentList(PublishedContent.AncestorsOrSelf(level));
        }

        #endregion

        #region Descendants

        public DynamicPublishedContentList Descendants(string contentTypeAlias)
        {
            return new DynamicPublishedContentList(PublishedContent.Descendants(contentTypeAlias));
        }
        public DynamicPublishedContentList Descendants(int level)
        {
            return new DynamicPublishedContentList(PublishedContent.Descendants(level));
        }
        public DynamicPublishedContentList Descendants()
        {
            return new DynamicPublishedContentList(PublishedContent.Descendants());
        }
        public DynamicPublishedContentList DescendantsOrSelf(int level)
        {
            return new DynamicPublishedContentList(PublishedContent.DescendantsOrSelf(level));
        }
        public DynamicPublishedContentList DescendantsOrSelf(string contentTypeAlias)
        {
            return new DynamicPublishedContentList(PublishedContent.DescendantsOrSelf(contentTypeAlias));
        }
        public DynamicPublishedContentList DescendantsOrSelf()
        {
            return new DynamicPublishedContentList(PublishedContent.DescendantsOrSelf());
        }

        #endregion
        
        #region Traversal

		public DynamicPublishedContent Up()
		{
			return PublishedContent.Up().AsDynamicOrNull();
		}

		public DynamicPublishedContent Up(int number)
		{
            return PublishedContent.Up(number).AsDynamicOrNull();
		}

		public DynamicPublishedContent Up(string contentTypeAlias)
		{
            return PublishedContent.Up(contentTypeAlias).AsDynamicOrNull();
		}

		public DynamicPublishedContent Down()
		{
            return PublishedContent.Down().AsDynamicOrNull();
		}

		public DynamicPublishedContent Down(int number)
		{
            return PublishedContent.Down(number).AsDynamicOrNull();
		}

		public DynamicPublishedContent Down(string contentTypeAlias)
		{
            return PublishedContent.Down(contentTypeAlias).AsDynamicOrNull();
		}

		public DynamicPublishedContent Next()
		{
            return PublishedContent.Next().AsDynamicOrNull();
		}

		public DynamicPublishedContent Next(int number)
		{
            return PublishedContent.Next(number).AsDynamicOrNull();
		}

		public DynamicPublishedContent Next(string contentTypeAlias)
		{
            return PublishedContent.Next(contentTypeAlias).AsDynamicOrNull();
		}

		public DynamicPublishedContent Previous()
		{
            return PublishedContent.Previous().AsDynamicOrNull();
		}

		public DynamicPublishedContent Previous(int number)
		{
            return PublishedContent.Previous(number).AsDynamicOrNull();
		}

		public DynamicPublishedContent Previous(string contentTypeAlias)
		{
            return PublishedContent.Previous(contentTypeAlias).AsDynamicOrNull();
		}

		public DynamicPublishedContent Sibling(int number)
		{
            return PublishedContent.Previous(number).AsDynamicOrNull();
		}

		public DynamicPublishedContent Sibling(string contentTypeAlias)
		{
			return PublishedContent.Previous(contentTypeAlias).AsDynamicOrNull();
		} 

		#endregion

		#region Parent

		public DynamicPublishedContent Parent
		{
			get
			{
			    return PublishedContent.Parent != null ? PublishedContent.Parent.AsDynamicOrNull() : null;
			}
		} 
		
		#endregion

        #region Children

        // we want to cache the dynamic list of children here
        // whether PublishedContent.Children itself is cached, is not our concern

        private DynamicPublishedContentList _children;

        public DynamicPublishedContentList Children
        {
            get { return _children ?? (_children = new DynamicPublishedContentList(PublishedContent.Children)); }
        }

	    public DynamicPublishedContent FirstChild()
	    {
	        return Children.FirstOrDefault<DynamicPublishedContent>();
	    }

	    public DynamicPublishedContent FirstChild(string alias)
	    {
	        return Children.FirstOrDefault<IPublishedContent>(x => x.DocumentTypeAlias == alias) as DynamicPublishedContent;
	    }
        
        #endregion

        // should probably cleanup what's below

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
