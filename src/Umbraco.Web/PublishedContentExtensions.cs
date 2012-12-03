using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Web;
using Examine.LuceneEngine.SearchCriteria;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;
using Umbraco.Web.Models;
using Umbraco.Web.Routing;
using umbraco;
using umbraco.cms.businesslogic;
using Umbraco.Core;
using umbraco.cms.businesslogic.template;
using umbraco.interfaces;

namespace Umbraco.Web
{
	/// <summary>
	/// Extension methods for IPublishedContent
	/// </summary>
	/// <remarks>
	/// These methods exist in the web project as we need access to web based classes like NiceUrl provider
	/// which is why they cannot exist in the Core project.
	/// </remarks>
	public static class PublishedContentExtensions
	{

		/// <summary>
		/// Converts an INode to an IPublishedContent item
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		internal static IPublishedContent ConvertFromNode(this INode node)
		{
			var umbHelper = new UmbracoHelper(UmbracoContext.Current);
			return umbHelper.TypedContent(node.Id);
		}

		/// <summary>
		/// Gets the NiceUrl for the content item
		/// </summary>
		/// <param name="doc"></param>
		/// <returns></returns>
		public static string NiceUrl(this IPublishedContent doc)
		{
			var umbHelper = new UmbracoHelper(UmbracoContext.Current);
			return umbHelper.NiceUrl(doc.Id);
		}

		/// <summary>
		/// Gets the NiceUrlWithDomain for the content item
		/// </summary>
		/// <param name="doc"></param>
		/// <returns></returns>
		public static string NiceUrlWithDomain(this IPublishedContent doc)
		{
			var umbHelper = new UmbracoHelper(UmbracoContext.Current);
			return umbHelper.NiceUrlWithDomain(doc.Id);
		}

		/// <summary>
		/// Returns the current template Alias
		/// </summary>
		/// <param name="doc"></param>
		/// <returns></returns>
		public static string GetTemplateAlias(this IPublishedContent doc)
		{
			var template = Template.GetTemplate(doc.TemplateId);
			return template.Alias;			
		}

		#region Search
		public static IEnumerable<IPublishedContent> Search(this IPublishedContent d, string term, bool useWildCards = true, string searchProvider = null)
		{
			var searcher = Examine.ExamineManager.Instance.DefaultSearchProvider;
			if (!string.IsNullOrEmpty(searchProvider))
				searcher = Examine.ExamineManager.Instance.SearchProviderCollection[searchProvider];

			var t = term.Escape().Value;
			if (useWildCards)
				t = term.MultipleCharacterWildcard().Value;

			string luceneQuery = "+__Path:(" + d.Path.Replace("-", "\\-") + "*) +" + t;
			var crit = searcher.CreateSearchCriteria().RawQuery(luceneQuery);

			return d.Search(crit, searcher);
		}

		public static IEnumerable<IPublishedContent> SearchDescendants(this IPublishedContent d, string term, bool useWildCards = true, string searchProvider = null)
		{
			return d.Search(term, useWildCards, searchProvider);
		}

		public static IEnumerable<IPublishedContent> SearchChildren(this IPublishedContent d, string term, bool useWildCards = true, string searchProvider = null)
		{
			var searcher = Examine.ExamineManager.Instance.DefaultSearchProvider;
			if (!string.IsNullOrEmpty(searchProvider))
				searcher = Examine.ExamineManager.Instance.SearchProviderCollection[searchProvider];

			var t = term.Escape().Value;
			if (useWildCards)
				t = term.MultipleCharacterWildcard().Value;

			string luceneQuery = "+parentID:" + d.Id.ToString() + " +" + t;
			var crit = searcher.CreateSearchCriteria().RawQuery(luceneQuery);

			return d.Search(crit, searcher);
		}

		public static IEnumerable<IPublishedContent> Search(this IPublishedContent d, Examine.SearchCriteria.ISearchCriteria criteria, Examine.Providers.BaseSearchProvider searchProvider = null)
		{
			var s = Examine.ExamineManager.Instance.DefaultSearchProvider;
			if (searchProvider != null)
				s = searchProvider;

			var results = s.Search(criteria);
			return results.ConvertSearchResultToPublishedContent(PublishedContentStoreResolver.Current.PublishedContentStore);
		} 
		#endregion

		#region List Extensions

		public static IQueryable<IPublishedContent> OrderBy(this IEnumerable<IPublishedContent> list, string predicate)
		{
			var dList = new DynamicPublishedContentList(list);
			return dList.OrderBy<DynamicPublishedContent>(predicate);
		}

		public static IQueryable<IPublishedContent> Where(this IEnumerable<IPublishedContent> list, string predicate)
		{
			var dList = new DynamicPublishedContentList(list);
			return dList.Where<DynamicPublishedContent>(predicate);
		}

		public static IEnumerable<IGrouping<object, IPublishedContent>> GroupBy(this IEnumerable<IPublishedContent> list, string predicate)
		{
			var dList = new DynamicPublishedContentList(list);
			return dList.GroupBy(predicate);
		}

		public static IQueryable Select(this IEnumerable<IPublishedContent> list, string predicate, params object[] values)
		{
			var dList = new DynamicPublishedContentList(list);
			return dList.Select(predicate);
		}

		#endregion

		public static dynamic AsDynamic(this IPublishedContent doc)
		{
			if (doc == null) throw new ArgumentNullException("doc");
			var dd = new DynamicPublishedContent(doc);
			return dd.AsDynamic();
		}

		/// <summary>
		/// Converts a IPublishedContent to a DynamicPublishedContent and tests for null
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>
		internal static DynamicPublishedContent AsDynamicPublishedContent(this IPublishedContent content)
		{
			if (content == null)
				return null;
			return new DynamicPublishedContent(content);
		}

		#region Where

		public static HtmlString Where(this IPublishedContent doc, string predicate, string valueIfTrue)
		{
			if (doc == null) throw new ArgumentNullException("doc");
			return doc.Where(predicate, valueIfTrue, string.Empty);
		}

		public static HtmlString Where(this IPublishedContent doc, string predicate, string valueIfTrue, string valueIfFalse)
		{
			if (doc == null) throw new ArgumentNullException("doc");
			if (doc.Where(predicate))
			{
				return new HtmlString(valueIfTrue);
			}
			return new HtmlString(valueIfFalse);
		}
		public static bool Where(this IPublishedContent doc, string predicate)
		{
			if (doc == null) throw new ArgumentNullException("doc");
			//Totally gonna cheat here
			var dynamicDocumentList = new DynamicPublishedContentList();
			dynamicDocumentList.Add(doc.AsDynamicPublishedContent());
			var filtered = dynamicDocumentList.Where<DynamicPublishedContent>(predicate);
			if (Queryable.Count(filtered) == 1)
			{
				//this node matches the predicate
				return true;
			}
			return false;
		}

		#endregion

		#region Position/Index
		public static int Position(this IPublishedContent content)
		{
			return content.Index();
		}
		public static int Index(this IPublishedContent content)
		{
			//get the root docs if parent is null
			var ownersList = content.Parent == null
								 ? PublishedContentStoreResolver.Current.PublishedContentStore.GetRootDocuments(UmbracoContext.Current)
								 : content.Parent.Children;

			var container = ownersList.ToList();
			int currentIndex = container.FindIndex(n => n.Id == content.Id);
			if (currentIndex != -1)
			{
				return currentIndex;
			}
			else
			{
				throw new IndexOutOfRangeException(string.Format("Node {0} belongs to a DynamicDocumentList but could not retrieve the index for it's position in the list", content.Id));
			}
		} 
		#endregion

		#region Is Helpers

		public static bool IsDocumentType(this IPublishedContent content, string docTypeAlias)
		{
			return content.DocumentTypeAlias == docTypeAlias;
		}

		public static bool IsNull(this IPublishedContent content, string alias, bool recursive)
		{
			var prop = content.GetProperty(alias, recursive);
			if (prop == null) return true;
			return ((PropertyResult)prop).HasValue();
		}
		public static bool IsNull(this IPublishedContent content, string alias)
		{
			return content.IsNull(alias, false);
		}
		public static bool IsFirst(this IPublishedContent content)
		{
			return content.IsHelper(n => n.Index() == 0);
		}
		public static HtmlString IsFirst(this IPublishedContent content, string valueIfTrue)
		{
			return content.IsHelper(n => n.Index() == 0, valueIfTrue);
		}
		public static HtmlString IsFirst(this IPublishedContent content, string valueIfTrue, string valueIfFalse)
		{
			return content.IsHelper(n => n.Index() == 0, valueIfTrue, valueIfFalse);
		}
		public static bool IsNotFirst(this IPublishedContent content)
		{
			return !content.IsHelper(n => n.Index() == 0);
		}
		public static HtmlString IsNotFirst(this IPublishedContent content, string valueIfTrue)
		{
			return content.IsHelper(n => n.Index() != 0, valueIfTrue);
		}
		public static HtmlString IsNotFirst(this IPublishedContent content, string valueIfTrue, string valueIfFalse)
		{
			return content.IsHelper(n => n.Index() != 0, valueIfTrue, valueIfFalse);
		}
		public static bool IsPosition(this IPublishedContent content, int index)
		{
			return content.IsHelper(n => n.Index() == index);
		}
		public static HtmlString IsPosition(this IPublishedContent content, int index, string valueIfTrue)
		{
			return content.IsHelper(n => n.Index() == index, valueIfTrue);
		}
		public static HtmlString IsPosition(this IPublishedContent content, int index, string valueIfTrue, string valueIfFalse)
		{
			return content.IsHelper(n => n.Index() == index, valueIfTrue, valueIfFalse);
		}
		public static bool IsModZero(this IPublishedContent content, int modulus)
		{
			return content.IsHelper(n => n.Index() % modulus == 0);
		}
		public static HtmlString IsModZero(this IPublishedContent content, int modulus, string valueIfTrue)
		{
			return content.IsHelper(n => n.Index() % modulus == 0, valueIfTrue);
		}
		public static HtmlString IsModZero(this IPublishedContent content, int modulus, string valueIfTrue, string valueIfFalse)
		{
			return content.IsHelper(n => n.Index() % modulus == 0, valueIfTrue, valueIfFalse);
		}

		public static bool IsNotModZero(this IPublishedContent content, int modulus)
		{
			return content.IsHelper(n => n.Index() % modulus != 0);
		}
		public static HtmlString IsNotModZero(this IPublishedContent content, int modulus, string valueIfTrue)
		{
			return content.IsHelper(n => n.Index() % modulus != 0, valueIfTrue);
		}
		public static HtmlString IsNotModZero(this IPublishedContent content, int modulus, string valueIfTrue, string valueIfFalse)
		{
			return content.IsHelper(n => n.Index() % modulus != 0, valueIfTrue, valueIfFalse);
		}
		public static bool IsNotPosition(this IPublishedContent content, int index)
		{
			return !content.IsHelper(n => n.Index() == index);
		}
		public static HtmlString IsNotPosition(this IPublishedContent content, int index, string valueIfTrue)
		{
			return content.IsHelper(n => n.Index() != index, valueIfTrue);
		}
		public static HtmlString IsNotPosition(this IPublishedContent content, int index, string valueIfTrue, string valueIfFalse)
		{
			return content.IsHelper(n => n.Index() != index, valueIfTrue, valueIfFalse);
		}
		public static bool IsLast(this IPublishedContent content)
		{
			//get the root docs if parent is null
			var ownersList = content.Parent == null
								 ? PublishedContentStoreResolver.Current.PublishedContentStore.GetRootDocuments(UmbracoContext.Current)
								 : content.Parent.Children;
			var count = ownersList.Count();
			return content.IsHelper(n => n.Index() == count - 1);
		}
		public static HtmlString IsLast(this IPublishedContent content, string valueIfTrue)
		{
			//get the root docs if parent is null
			var ownersList = content.Parent == null
								 ? PublishedContentStoreResolver.Current.PublishedContentStore.GetRootDocuments(UmbracoContext.Current)
								 : content.Parent.Children;
			var count = ownersList.Count();
			return content.IsHelper(n => n.Index() == count - 1, valueIfTrue);
		}
		public static HtmlString IsLast(this IPublishedContent content, string valueIfTrue, string valueIfFalse)
		{
			//get the root docs if parent is null
			var ownersList = content.Parent == null
								 ? PublishedContentStoreResolver.Current.PublishedContentStore.GetRootDocuments(UmbracoContext.Current)
								 : content.Parent.Children;
			var count = ownersList.Count();
			return content.IsHelper(n => n.Index() == count - 1, valueIfTrue, valueIfFalse);
		}
		public static bool IsNotLast(this IPublishedContent content)
		{
			//get the root docs if parent is null
			var ownersList = content.Parent == null
								 ? PublishedContentStoreResolver.Current.PublishedContentStore.GetRootDocuments(UmbracoContext.Current)
								 : content.Parent.Children;
			var count = ownersList.Count();
			return !content.IsHelper(n => n.Index() == count - 1);
		}
		public static HtmlString IsNotLast(this IPublishedContent content, string valueIfTrue)
		{
			//get the root docs if parent is null
			var ownersList = content.Parent == null
								 ? PublishedContentStoreResolver.Current.PublishedContentStore.GetRootDocuments(UmbracoContext.Current)
								 : content.Parent.Children;
			var count = ownersList.Count();
			return content.IsHelper(n => n.Index() != count - 1, valueIfTrue);
		}
		public static HtmlString IsNotLast(this IPublishedContent content, string valueIfTrue, string valueIfFalse)
		{
			//get the root docs if parent is null
			var ownersList = content.Parent == null
								 ? PublishedContentStoreResolver.Current.PublishedContentStore.GetRootDocuments(UmbracoContext.Current)
								 : content.Parent.Children;
			var count = ownersList.Count();
			return content.IsHelper(n => n.Index() != count - 1, valueIfTrue, valueIfFalse);
		}
		public static bool IsEven(this IPublishedContent content)
		{
			return content.IsHelper(n => n.Index() % 2 == 0);
		}
		public static HtmlString IsEven(this IPublishedContent content, string valueIfTrue)
		{
			return content.IsHelper(n => n.Index() % 2 == 0, valueIfTrue);
		}
		public static HtmlString IsEven(this IPublishedContent content, string valueIfTrue, string valueIfFalse)
		{
			return content.IsHelper(n => n.Index() % 2 == 0, valueIfTrue, valueIfFalse);
		}
		public static bool IsOdd(this IPublishedContent content)
		{
			return content.IsHelper(n => n.Index() % 2 == 1);
		}
		public static HtmlString IsOdd(this IPublishedContent content, string valueIfTrue)
		{
			return content.IsHelper(n => n.Index() % 2 == 1, valueIfTrue);
		}
		public static HtmlString IsOdd(this IPublishedContent content, string valueIfTrue, string valueIfFalse)
		{
			return content.IsHelper(n => n.Index() % 2 == 1, valueIfTrue, valueIfFalse);
		}
		public static bool IsEqual(this IPublishedContent content, IPublishedContent other)
		{
			return content.IsHelper(n => n.Id == other.Id);
		}
		public static HtmlString IsEqual(this IPublishedContent content, IPublishedContent other, string valueIfTrue)
		{
			return content.IsHelper(n => n.Id == other.Id, valueIfTrue);
		}
		public static HtmlString IsEqual(this IPublishedContent content, IPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
			return content.IsHelper(n => n.Id == other.Id, valueIfTrue, valueIfFalse);
		}
		public static bool IsNotEqual(this IPublishedContent content, IPublishedContent other)
		{
			return content.IsHelper(n => n.Id != other.Id);
		}
		public static HtmlString IsNotEqual(this IPublishedContent content, IPublishedContent other, string valueIfTrue)
		{
			return content.IsHelper(n => n.Id != other.Id, valueIfTrue);
		}
		public static HtmlString IsNotEqual(this IPublishedContent content, IPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
			return content.IsHelper(n => n.Id != other.Id, valueIfTrue, valueIfFalse);
		}
		public static bool IsDescendant(this IPublishedContent content, IPublishedContent other)
		{
			var ancestors = content.Ancestors();
			return content.IsHelper(n => ancestors.FirstOrDefault(ancestor => ancestor.Id == other.Id) != null);
		}
		public static HtmlString IsDescendant(this IPublishedContent content, IPublishedContent other, string valueIfTrue)
		{
			var ancestors = content.Ancestors();
			return content.IsHelper(n => ancestors.FirstOrDefault(ancestor => ancestor.Id == other.Id) != null, valueIfTrue);
		}
		public static HtmlString IsDescendant(this IPublishedContent content, IPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
			var ancestors = content.Ancestors();
			return content.IsHelper(n => ancestors.FirstOrDefault(ancestor => ancestor.Id == other.Id) != null, valueIfTrue, valueIfFalse);
		}
		public static bool IsDescendantOrSelf(this IPublishedContent content, IPublishedContent other)
		{
			var ancestors = content.AncestorsOrSelf();
			return content.IsHelper(n => ancestors.FirstOrDefault(ancestor => ancestor.Id == other.Id) != null);
		}
		public static HtmlString IsDescendantOrSelf(this IPublishedContent content, DynamicPublishedContent other, string valueIfTrue)
		{
			var ancestors = content.AncestorsOrSelf();
			return content.IsHelper(n => ancestors.FirstOrDefault(ancestor => ancestor.Id == other.Id) != null, valueIfTrue);
		}
		public static HtmlString IsDescendantOrSelf(this IPublishedContent content, DynamicPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
			var ancestors = content.AncestorsOrSelf();
			return content.IsHelper(n => ancestors.FirstOrDefault(ancestor => ancestor.Id == other.Id) != null, valueIfTrue, valueIfFalse);
		}
		public static bool IsAncestor(this IPublishedContent content, DynamicPublishedContent other)
		{
			var descendants = content.Descendants();
			return content.IsHelper(n => descendants.FirstOrDefault(descendant => descendant.Id == other.Id) != null);
		}
		public static HtmlString IsAncestor(this IPublishedContent content, DynamicPublishedContent other, string valueIfTrue)
		{
			var descendants = content.Descendants();
			return content.IsHelper(n => descendants.FirstOrDefault(descendant => descendant.Id == other.Id) != null, valueIfTrue);
		}
		public static HtmlString IsAncestor(this IPublishedContent content, DynamicPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
			var descendants = content.Descendants();
			return content.IsHelper(n => descendants.FirstOrDefault(descendant => descendant.Id == other.Id) != null, valueIfTrue, valueIfFalse);
		}
		public static bool IsAncestorOrSelf(this IPublishedContent content, DynamicPublishedContent other)
		{
			var descendants = content.DescendantsOrSelf();
			return content.IsHelper(n => descendants.FirstOrDefault(descendant => descendant.Id == other.Id) != null);
		}
		public static HtmlString IsAncestorOrSelf(this IPublishedContent content, DynamicPublishedContent other, string valueIfTrue)
		{
			var descendants = content.DescendantsOrSelf();
			return content.IsHelper(n => descendants.FirstOrDefault(descendant => descendant.Id == other.Id) != null, valueIfTrue);
		}
		public static HtmlString IsAncestorOrSelf(this IPublishedContent content, DynamicPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
			var descendants = content.DescendantsOrSelf();
			return content.IsHelper(n => descendants.FirstOrDefault(descendant => descendant.Id == other.Id) != null, valueIfTrue, valueIfFalse);
		}
		private static bool IsHelper(this IPublishedContent content, Func<IPublishedContent, bool> test)
		{
			return test(content);
		}
		private static HtmlString IsHelper(this IPublishedContent content, Func<IPublishedContent, bool> test, string valueIfTrue)
		{
			return content.IsHelper(test, valueIfTrue, string.Empty);
		}
		private static HtmlString IsHelper(this IPublishedContent content, Func<IPublishedContent, bool> test, string valueIfTrue, string valueIfFalse)
		{
			return test(content) ? new HtmlString(valueIfTrue) : new HtmlString(valueIfFalse);
		}
		#endregion

		#region Ancestors

		public static IEnumerable<IPublishedContent> Ancestors(this IPublishedContent content, int level)
		{
			return content.Ancestors(n => n.Level <= level);
		}
		public static IEnumerable<IPublishedContent> Ancestors(this IPublishedContent content, string nodeTypeAlias)
		{
			return content.Ancestors(n => n.DocumentTypeAlias == nodeTypeAlias);
		}
		public static IEnumerable<IPublishedContent> Ancestors(this IPublishedContent content)
		{
			return content.Ancestors(n => true);
		}
		internal static IEnumerable<IPublishedContent> Ancestors(this IPublishedContent content, Func<IPublishedContent, bool> func)
		{
			var ancestorList = new List<IPublishedContent>();
			var node = content;
			while (node != null)
			{
				if (node.Level == 1) break;
				var parent = node.Parent;
				if (parent == null)
				{
					break;
				}
				if (content != parent)
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
			ancestorList.Reverse();
			return ancestorList;
		}

		public static IPublishedContent AncestorOrSelf(this IPublishedContent content)
		{
			//TODO: Why is this query like this??
			return content.AncestorOrSelf(node => node.Level == 1);
		}
		public static IPublishedContent AncestorOrSelf(this IPublishedContent content, int level)
		{
			return content.AncestorOrSelf(node => node.Level == level);
		}
		public static IPublishedContent AncestorOrSelf(this IPublishedContent content, string nodeTypeAlias)
		{
			return content.AncestorOrSelf(node => node.DocumentTypeAlias == nodeTypeAlias);
		}
		internal static IPublishedContent AncestorOrSelf(this IPublishedContent content, Func<IPublishedContent, bool> func)
		{
			var node = content;
			while (node != null)
			{
				if (func(node)) return node;
				var parent = node.Parent;
				if (parent == null)
				{
					return null;
				}
				if (content != parent)
				{
					node = parent;
				}
				else
				{
					return node;
				}
			}
			return null;
		}

		internal static IEnumerable<IPublishedContent> AncestorsOrSelf(this IPublishedContent content, Func<IPublishedContent, bool> func)
		{
			var ancestorList = new List<IPublishedContent>();
			var node = content;
			ancestorList.Add(node);
			while (node != null)
			{
				if (node.Level == 1) break;
				var parent = node.Parent;
				if (parent == null)
				{
					break;
				}
				if (content != parent)
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
			ancestorList.Reverse();
			return ancestorList;
		}
		public static IEnumerable<IPublishedContent> AncestorsOrSelf(this IPublishedContent content)
		{
			return content.AncestorsOrSelf(n => true);
		}
		public static IEnumerable<IPublishedContent> AncestorsOrSelf(this IPublishedContent content, string nodeTypeAlias)
		{
			return content.AncestorsOrSelf(n => n.DocumentTypeAlias == nodeTypeAlias);
		}
		internal static IEnumerable<IPublishedContent> AncestorsOrSelf(this IPublishedContent content, int level)
		{
			return content.AncestorsOrSelf(n => n.Level <= level);
		}

		#endregion

		#region Descendants
		public static IEnumerable<IPublishedContent> Descendants(this IPublishedContent content, string nodeTypeAlias)
		{
			return content.Descendants(p => p.DocumentTypeAlias == nodeTypeAlias);
		}
		public static IEnumerable<IPublishedContent> Descendants(this IPublishedContent content, int level)
		{
			return content.Descendants(p => p.Level >= level);
		}
		public static IEnumerable<IPublishedContent> Descendants(this IPublishedContent content)
		{
			return content.Descendants(n => true);
		}
		private static IEnumerable<IPublishedContent> Descendants(this IPublishedContent content, Func<IPublishedContent, bool> func)
		{
			return content.Children.Map(func, (IPublishedContent n) => n.Children);
		}
		public static IEnumerable<IPublishedContent> DescendantsOrSelf(this IPublishedContent content, int level)
		{
			return content.DescendantsOrSelf(p => p.Level >= level);
		}
		public static IEnumerable<IPublishedContent> DescendantsOrSelf(this IPublishedContent content, string nodeTypeAlias)
		{
			return content.DescendantsOrSelf(p => p.DocumentTypeAlias == nodeTypeAlias);
		}
		public static IEnumerable<IPublishedContent> DescendantsOrSelf(this IPublishedContent content)
		{
			return content.DescendantsOrSelf(p => true);
		}
		internal static IEnumerable<IPublishedContent> DescendantsOrSelf(this IPublishedContent content, Func<IPublishedContent, bool> func)
		{
			if (content != null)
			{
				var thisNode = new List<IPublishedContent>();
				if (func(content))
				{
					thisNode.Add(content);
				}
				var flattenedNodes = content.Children.Map(func, (IPublishedContent n) => n.Children);
				return thisNode.Concat(flattenedNodes).ToList().ConvertAll(dynamicBackingItem => new DynamicPublishedContent(dynamicBackingItem));
			}
			return Enumerable.Empty<IPublishedContent>();
		}
		#endregion

		#region Traversal

		public static IPublishedContent Up(this IPublishedContent content)
		{
			return content.Up(0);
		}
		public static IPublishedContent Up(this IPublishedContent content, int number)
		{
			if (number == 0)
			{
				return content.Parent;
			}
			while ((content = content.Parent) != null && --number >= 0) ;
			return content;
		}
		public static IPublishedContent Up(this IPublishedContent content, string nodeTypeAlias)
		{
			if (string.IsNullOrEmpty(nodeTypeAlias))
			{
				return content.Parent;
			}
			while ((content = content.Parent) != null && content.DocumentTypeAlias != nodeTypeAlias) ;
			return content;
		}
		public static IPublishedContent Down(this IPublishedContent content)
		{
			return content.Down(0);
		}
		public static IPublishedContent Down(this IPublishedContent content, int number)
		{
			var children = content.Children;
			if (number == 0)
			{
				return children.First();
			}
			var working = content;
			while (number-- >= 0)
			{
				working = children.First();
				children = new DynamicPublishedContentList(working.Children);
			}
			return working;
		}
		public static IPublishedContent Down(this IPublishedContent content, string nodeTypeAlias)
		{
			if (string.IsNullOrEmpty(nodeTypeAlias))
			{
				var children = content.Children;
				return children.First();
			}
			return content.Descendants(nodeTypeAlias).FirstOrDefault();
		}

		public static IPublishedContent Next(this IPublishedContent content)
		{
			return content.Next(0);
		}
		public static IPublishedContent Next(this IPublishedContent content, int number)
		{
			//get the root docs if parent is null
			var ownersList = content.Parent == null
				                 ? PublishedContentStoreResolver.Current.PublishedContentStore.GetRootDocuments(UmbracoContext.Current)
				                 : content.Parent.Children;

			var container = ownersList.ToList();
			var currentIndex = container.FindIndex(n => n.Id == content.Id);
			if (currentIndex != -1)
			{
				return container.ElementAtOrDefault(currentIndex + (number + 1));
			}
			throw new IndexOutOfRangeException(string.Format("Node {0} belongs to a DynamicNodeList but could not retrieve the index for it's position in the list", content.Id));
		}

		public static IPublishedContent Next(this IPublishedContent content, string nodeTypeAlias)
		{
			//get the root docs if parent is null
			var ownersList = content.Parent == null
				                 ? PublishedContentStoreResolver.Current.PublishedContentStore.GetRootDocuments(UmbracoContext.Current)
				                 : content.Parent.Children;

			var container = ownersList.ToList();
			var currentIndex = container.FindIndex(n => n.Id == content.Id);
			if (currentIndex != -1)
			{
				var newIndex = container.FindIndex(currentIndex, n => n.DocumentTypeAlias == nodeTypeAlias);
				return newIndex != -1 
					? container.ElementAt(newIndex) 
					: null;
			}
			throw new IndexOutOfRangeException(string.Format("Node {0} belongs to a DynamicNodeList but could not retrieve the index for it's position in the list", content.Id));
		}
		public static IPublishedContent Previous(this IPublishedContent content)
		{
			return content.Previous(0);
		}
		public static IPublishedContent Previous(this IPublishedContent content, int number)
		{
			//get the root docs if parent is null
			var ownersList = content.Parent == null
								 ? PublishedContentStoreResolver.Current.PublishedContentStore.GetRootDocuments(UmbracoContext.Current)
								 : content.Parent.Children;

			var container = ownersList.ToList();
			var currentIndex = container.FindIndex(n => n.Id == content.Id);
			if (currentIndex != -1)
			{
				return container.ElementAtOrDefault(currentIndex + (number - 1));
			}
			throw new IndexOutOfRangeException(string.Format("Node {0} belongs to a DynamicNodeList but could not retrieve the index for it's position in the list", content.Id));
		}
		public static IPublishedContent Previous(this IPublishedContent content, string nodeTypeAlias)
		{
			//get the root docs if parent is null
			var ownersList = content.Parent == null
				                 ? PublishedContentStoreResolver.Current.PublishedContentStore.GetRootDocuments(UmbracoContext.Current)
				                 : content.Parent.Children;

			var container = ownersList.ToList();
			int currentIndex = container.FindIndex(n => n.Id == content.Id);
			if (currentIndex != -1)
			{
				var previousNodes = container.Take(currentIndex).ToList();
				int newIndex = previousNodes.FindIndex(n => n.DocumentTypeAlias == nodeTypeAlias);
				if (newIndex != -1)
				{
					return container.ElementAt(newIndex);
				}
				return null;
			}
			throw new IndexOutOfRangeException(string.Format("Node {0} belongs to a DynamicNodeList but could not retrieve the index for it's position in the list", content.Id));
		}
		public static IPublishedContent Sibling(this IPublishedContent content, int number)
		{
			//get the root docs if parent is null
			var ownersList = content.Parent == null
								 ? PublishedContentStoreResolver.Current.PublishedContentStore.GetRootDocuments(UmbracoContext.Current)
								 : content.Parent.Children;

			var container = ownersList.ToList();
			var currentIndex = container.FindIndex(n => n.Id == content.Id);
			if (currentIndex != -1)
			{
				return container.ElementAtOrDefault(currentIndex + number);
			}
			throw new IndexOutOfRangeException(string.Format("Node {0} belongs to a DynamicNodeList but could not retrieve the index for it's position in the list", content.Id));
		}
		public static IPublishedContent Sibling(this IPublishedContent content, string nodeTypeAlias)
		{
			//get the root docs if parent is null
			var ownersList = content.Parent == null
				                 ? PublishedContentStoreResolver.Current.PublishedContentStore.GetRootDocuments(UmbracoContext.Current)
				                 : content.Parent.Children;

			var container = ownersList.ToList();
			var currentIndex = container.FindIndex(n => n.Id == content.Id);
			if (currentIndex != -1)
			{
				var workingIndex = currentIndex + 1;
				while (workingIndex != currentIndex)
				{
					var working = container.ElementAtOrDefault(workingIndex);
					if (working != null && working.DocumentTypeAlias == nodeTypeAlias)
					{
						return working;
					}
					workingIndex++;
					if (workingIndex > container.Count)
					{
						workingIndex = 0;
					}
				}
				return null;
			}
			throw new IndexOutOfRangeException(string.Format("Node {0} belongs to a DynamicNodeList but could not retrieve the index for it's position in the list", content.Id));
		}
		#endregion

		/// <summary>
		/// Method to return the Children of the content item
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		/// <remarks>
		/// This method exists for consistency, it is the same as calling content.Children as a property.
		/// </remarks>
		public static IEnumerable<IPublishedContent> Children(this IPublishedContent p)
		{
			return p.Children;
		} 

		/// <summary>
		/// Returns a DataTable object for the IPublishedContent
		/// </summary>
		/// <param name="d"></param>
		/// <param name="nodeTypeAliasFilter"></param>
		/// <returns></returns>
		public static DataTable ChildrenAsTable(this IPublishedContent d, string nodeTypeAliasFilter = "")
		{
			return GenerateDataTable(d, nodeTypeAliasFilter);
		}

		/// <summary>
		/// Generates the DataTable for the IPublishedContent
		/// </summary>
		/// <param name="node"></param>
		/// <param name="nodeTypeAliasFilter"> </param>
		/// <returns></returns>
		private static DataTable GenerateDataTable(IPublishedContent node, string nodeTypeAliasFilter = "")
		{
			var firstNode = nodeTypeAliasFilter.IsNullOrWhiteSpace()
			                	? node.Children.Any()
			                	  	? node.Children.ElementAt(0)
			                	  	: null
			                	: node.Children.FirstOrDefault(x => x.DocumentTypeAlias == nodeTypeAliasFilter);
			if (firstNode == null)
				return new DataTable(); //no children found 

			var urlProvider = UmbracoContext.Current.RoutingContext.NiceUrlProvider;

			//use new utility class to create table so that we don't have to maintain code in many places, just one
			var dt = Umbraco.Core.DataTableExtensions.GenerateDataTable(
				//pass in the alias of the first child node since this is the node type we're rendering headers for
				firstNode.DocumentTypeAlias,
				//pass in the callback to extract the Dictionary<string, string> of all defined aliases to their names
				alias => GetPropertyAliasesAndNames(alias),
				//pass in a callback to populate the datatable, yup its a bit ugly but it's already legacy and we just want to maintain code in one place.
				() =>
					{
						//create all row data
						var tableData = Umbraco.Core.DataTableExtensions.CreateTableData();
						//loop through each child and create row data for it
						foreach (var n in node.Children)
						{
							if (!nodeTypeAliasFilter.IsNullOrWhiteSpace())
							{
								if (n.DocumentTypeAlias != nodeTypeAliasFilter)
									continue; //skip this one, it doesn't match the filter
							}

							var standardVals = new Dictionary<string, object>()
								{
									{"Id", n.Id},
									{"NodeName", n.Name},
									{"NodeTypeAlias", n.DocumentTypeAlias},
									{"CreateDate", n.CreateDate},
									{"UpdateDate", n.UpdateDate},
									{"CreatorName", n.CreatorName},
									{"WriterName", n.WriterName},
									{"Url", urlProvider.GetNiceUrl(n.Id)}
								};
							var userVals = new Dictionary<string, object>();
							foreach (var p in from IPublishedContentProperty p in n.Properties where p.Value != null select p)
							{
								userVals[p.Alias] = p.Value;
							}
							//add the row data
							Umbraco.Core.DataTableExtensions.AddRowData(tableData, standardVals, userVals);
						}
						return tableData;
					}
				);
			return dt;
		}

		private static Func<string, Dictionary<string, string>> _getPropertyAliasesAndNames;
		
		/// <summary>
		/// This is used only for unit tests to set the delegate to look up aliases/names dictionary of a content type
		/// </summary>
		internal static Func<string, Dictionary<string, string>> GetPropertyAliasesAndNames
		{
			get
			{
				return _getPropertyAliasesAndNames ?? (_getPropertyAliasesAndNames = alias =>
					{
						var userFields = ContentType.GetAliasesAndNames(alias);
						//ensure the standard fields are there
						var allFields = new Dictionary<string, string>()
							{
								{"Id", "Id"},
								{"NodeName", "NodeName"},
								{"NodeTypeAlias", "NodeTypeAlias"},
								{"CreateDate", "CreateDate"},
								{"UpdateDate", "UpdateDate"},
								{"CreatorName", "CreatorName"},
								{"WriterName", "WriterName"},
								{"Url", "Url"}
							};
						foreach (var f in userFields.Where(f => !allFields.ContainsKey(f.Key)))
						{
							allFields.Add(f.Key, f.Value);
						}
						return allFields;
					});
			}
			set { _getPropertyAliasesAndNames = value; }
		}
	}
}