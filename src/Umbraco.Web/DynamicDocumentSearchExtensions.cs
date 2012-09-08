using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Examine;
using Examine.LuceneEngine.SearchCriteria;
using Umbraco.Core.Dynamics;

namespace Umbraco.Web
{
	/// <summary>
	/// DynamicDocument extension methods for searching using Examine
	/// </summary>
	public static class DynamicDocumentSearchExtensions
	{
		public static DynamicDocumentList Search(this DynamicDocument d, string term, bool useWildCards = true, string searchProvider = null)
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

		public static DynamicDocumentList SearchDescendants(this DynamicDocument d, string term, bool useWildCards = true, string searchProvider = null)
		{
			return d.Search(term, useWildCards, searchProvider);
		}

		public static DynamicDocumentList SearchChildren(this DynamicDocument d, string term, bool useWildCards = true, string searchProvider = null)
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

		public static DynamicDocumentList Search(this DynamicDocument d, Examine.SearchCriteria.ISearchCriteria criteria, Examine.Providers.BaseSearchProvider searchProvider = null)
		{
			var s = Examine.ExamineManager.Instance.DefaultSearchProvider;
			if (searchProvider != null)
				s = searchProvider;

			var results = s.Search(criteria);
			return ConvertSearchResultToDynamicNode(results);
		}

		private static DynamicDocumentList ConvertSearchResultToDynamicNode(IEnumerable<SearchResult> results)
		{
			var list = new DynamicDocumentList();
			var xd = new XmlDocument();

			foreach (var result in results.OrderByDescending(x => x.Score))
			{
				var doc = PublishedContentStoreResolver.Current.PublishedContentStore.GetDocumentById(
					UmbracoContext.Current,
					result.Id);
				if (doc == null) continue; //skip if this doesn't exist in the cache				
				doc.Properties.Add(
					new PropertyResult("examineScore", result.Score.ToString(), Guid.Empty, PropertyResultType.CustomProperty));
				var dynamicDoc = new DynamicDocument(doc);				
				list.Add(dynamicDoc);
			}
			return list;
		}
	}
}