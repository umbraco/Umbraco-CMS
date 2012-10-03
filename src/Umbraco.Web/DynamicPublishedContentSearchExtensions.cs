using Examine.LuceneEngine.SearchCriteria;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;
using Umbraco.Web.Models;

namespace Umbraco.Web
{
	/// <summary>
	/// DynamicPublishedContent extension methods for searching using Examine
	/// </summary>
	public static class DynamicPublishedContentSearchExtensions
	{
		public static DynamicPublishedContentList Search(this DynamicPublishedContent d, string term, bool useWildCards = true, string searchProvider = null)
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

		public static DynamicPublishedContentList SearchDescendants(this DynamicPublishedContent d, string term, bool useWildCards = true, string searchProvider = null)
		{
			return d.Search(term, useWildCards, searchProvider);
		}

		public static DynamicPublishedContentList SearchChildren(this DynamicPublishedContent d, string term, bool useWildCards = true, string searchProvider = null)
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

		public static DynamicPublishedContentList Search(this DynamicPublishedContent d, Examine.SearchCriteria.ISearchCriteria criteria, Examine.Providers.BaseSearchProvider searchProvider = null)
		{
			var s = Examine.ExamineManager.Instance.DefaultSearchProvider;
			if (searchProvider != null)
				s = searchProvider;

			var results = s.Search(criteria);
			return results.ConvertSearchResultToDynamicDocument(PublishedContentStoreResolver.Current.PublishedContentStore);
		}

		
	}
}