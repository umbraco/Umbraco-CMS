using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Web;
using umbraco.interfaces;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Web;

namespace umbraco.MacroEngines.Library
{
	public class RazorLibraryCore
    {
        private readonly INode _node;
    	private readonly UmbracoHelper _umbracoHelper;
        private readonly HtmlStringUtilities _stringUtilities = new HtmlStringUtilities();
		
		/// <summary>
		/// An empty HtmlHelper with a blank ViewContext, used only to access some htmlHelper extension methods
		/// </summary>
		private readonly HtmlHelper _htmlHelper;

        public INode Node
        {
            get { return _node; }
        }
        public RazorLibraryCore(INode node)
        {
            this._node = node;
			_umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
			_htmlHelper = new HtmlHelper(new ViewContext(), new ViewPage());
        }

        public dynamic NodeById(int Id)
        {
            var node = new DynamicNode(Id);
            if (node.Id == 0) return new DynamicNull();
            return node;
        }
        public dynamic NodeById(string Id)
        {
            var node = new DynamicNode(Id);
            if (node.Id == 0) return new DynamicNull();
            return node;
        }
        public dynamic NodeById(DynamicNull Id)
        {
            return new DynamicNull();
        }
        public dynamic NodeById(object Id)
        {
            if (Id.GetType() == typeof(DynamicNull))
            {
                return new DynamicNull();
            }
            var node = new DynamicNode(Id);
            if (node.Id == 0) return new DynamicNull();
            return node;
        }
        public dynamic NodesById(List<object> Ids)
        {
            List<DynamicNode> nodes = new List<DynamicNode>();
            foreach (object eachId in Ids)
                nodes.Add(new DynamicNode(eachId));
            return new DynamicNodeList(nodes);
        }
        public dynamic NodesById(List<int> Ids)
        {
            List<DynamicNode> nodes = new List<DynamicNode>();
            foreach (int eachId in Ids)
                nodes.Add(new DynamicNode(eachId));
            return new DynamicNodeList(nodes);
        }
        public dynamic NodesById(List<int> Ids, DynamicBackingItemType ItemType)
        {
            List<DynamicNode> nodes = new List<DynamicNode>();
            foreach (int eachId in Ids)
                nodes.Add(new DynamicNode(eachId, ItemType));
            return new DynamicNodeList(nodes);
        }
        public dynamic NodesById(params object[] Ids)
        {
            return NodesById(Ids.ToList());
        }
        public dynamic MediaById(DynamicNull Id)
        {
            return new DynamicNull();
        }
        public dynamic MediaById(int Id)
        {
            var ebm = ExamineBackedMedia.GetUmbracoMedia(Id);
            if (ebm != null && ebm.Id == 0)
            {
                return new DynamicNull();
            }
            return new DynamicMedia(new DynamicBackingItem(ebm));
        }
        public dynamic MediaById(string Id)
        {
            int mediaId = 0;
            if (int.TryParse(Id, out mediaId))
            {
                return MediaById(mediaId);
            }
            return new DynamicNull();
        }
        public dynamic MediaById(object Id)
        {
            if (Id.GetType() == typeof(DynamicNull))
            {
                return new DynamicNull();
            }
            int mediaId = 0;
            if (int.TryParse(string.Format("{0}", Id), out mediaId))
            {
                return MediaById(mediaId);
            }
            return null;
        }
        public dynamic MediaById(List<object> Ids)
        {
            List<DynamicNode> nodes = new List<DynamicNode>();
            foreach (object eachId in Ids)
                nodes.Add(MediaById(eachId));
            return new DynamicNodeList(nodes);
        }
        public dynamic MediaById(List<int> Ids)
        {
            List<DynamicNode> nodes = new List<DynamicNode>();
            foreach (int eachId in Ids)
                nodes.Add(MediaById(eachId));
            return new DynamicNodeList(nodes);
        }
        public dynamic MediaById(params object[] Ids)
        {
            return MediaById(Ids.ToList());
        }


        public dynamic Search(string term, bool useWildCards = true, string searchProvider = null)
        {
            var searcher = Examine.ExamineManager.Instance.DefaultSearchProvider;
            if (!string.IsNullOrEmpty(searchProvider))
                searcher = Examine.ExamineManager.Instance.SearchProviderCollection[searchProvider];

            var results = searcher.Search(term, useWildCards);
            return ExamineSearchUtill.ConvertSearchResultToDynamicNode(results);

            //TODO: Does NOT return legacy DynamicNodeList, old code is back in place

            ////wraps the functionality in UmbracoHelper but still returns the legacy DynamicNodeList
            //var nodes = ((DynamicPublishedContentList)_umbracoHelper.Search(term, useWildCards, searchProvider))
            //    .Select(x => x.ConvertToNode());
            //return new DynamicNodeList(nodes);
        }

        public dynamic Search(Examine.SearchCriteria.ISearchCriteria criteria, Examine.Providers.BaseSearchProvider searchProvider = null)
        {
            var s = Examine.ExamineManager.Instance.DefaultSearchProvider;
            if (searchProvider != null)
                s = searchProvider;

            var results = s.Search(criteria);
            return ExamineSearchUtill.ConvertSearchResultToDynamicNode(results);

            //TODO: Does NOT return legacy DynamicNodeList, old code is back in place

            ////wraps the functionality in UmbracoHelper but still returns the legacy DynamicNodeList
            //var nodes = ((DynamicPublishedContentList) _umbracoHelper.Search(criteria, searchProvider))
            //    .Select(x => x.ConvertToNode());
            //return new DynamicNodeList(nodes);
        }


        public T As<T>() where T : class
        {
            return (this as T);
        }

        public dynamic ToDynamicXml(string xml)
        {
        	return _umbracoHelper.ToDynamicXml(xml);
        }

        public dynamic ToDynamicXml(XElement xElement)
		{
			return _umbracoHelper.ToDynamicXml(xElement);
		}

        public dynamic ToDynamicXml(XPathNodeIterator xpni)
		{
			return _umbracoHelper.ToDynamicXml(xpni);
		}

        public string Coalesce(params object[] args)
        {
            return _stringUtilities.Coalesce<DynamicNull>(args);
        }

        public string Concatenate(params object[] args)
        {
            return _stringUtilities.Concatenate<DynamicNull>(args);
        }
        public string Join(string seperator, params object[] args)
        {
            return _stringUtilities.Join<DynamicNull>(seperator, args);
        }

        public HtmlString If(bool test, string valueIfTrue, string valueIfFalse)
        {
        	return _umbracoHelper.If(test, valueIfTrue, valueIfFalse);
        }
        public HtmlString If(bool test, string valueIfTrue)
        {
        	return _umbracoHelper.If(test, valueIfTrue);
        }

		public Umbraco.Web.Mvc.HtmlTagWrapper Wrap(string tag, string innerText, params Umbraco.Web.Mvc.IHtmlTagWrapper[] children)
		{
			return _htmlHelper.Wrap(tag, innerText, children);
        }
		public Umbraco.Web.Mvc.HtmlTagWrapper Wrap(string tag, object inner, object anonymousAttributes, params Umbraco.Web.Mvc.IHtmlTagWrapper[] children)
		{
			return _htmlHelper.Wrap(tag, inner, anonymousAttributes, children);
		}
		public Umbraco.Web.Mvc.HtmlTagWrapper Wrap(string tag, object inner)
		{
			return _htmlHelper.Wrap(tag, inner);
		}
		public Umbraco.Web.Mvc.HtmlTagWrapper Wrap(string tag, string innerText, object anonymousAttributes, params Umbraco.Web.Mvc.IHtmlTagWrapper[] children)
		{
			return _htmlHelper.Wrap(tag, innerText, anonymousAttributes, children);
		}
		public Umbraco.Web.Mvc.HtmlTagWrapper Wrap(bool visible, string tag, string innerText, object anonymousAttributes, params Umbraco.Web.Mvc.IHtmlTagWrapper[] children)
		{
			return _htmlHelper.Wrap(visible, tag, innerText, anonymousAttributes, children);
		}

        public IHtmlString Truncate(IHtmlString html, int length)
        {
            return Truncate(html.ToHtmlString(), length, true, false);
        }
        public IHtmlString Truncate(IHtmlString html, int length, bool addElipsis)
        {
            return Truncate(html.ToHtmlString(), length, addElipsis, false);
        }
        public IHtmlString Truncate(IHtmlString html, int length, bool addElipsis, bool treatTagsAsContent)
        {
            return Truncate(html.ToHtmlString(), length, addElipsis, treatTagsAsContent);
        }
        public IHtmlString Truncate(DynamicNull html, int length)
        {
            return new HtmlString(string.Empty);
        }
        public IHtmlString Truncate(DynamicNull html, int length, bool addElipsis)
        {
            return new HtmlString(string.Empty);
        }
        public IHtmlString Truncate(DynamicNull html, int length, bool addElipsis, bool treatTagsAsContent)
        {
            return new HtmlString(string.Empty);
        }
        public IHtmlString Truncate(string html, int length)
        {
            return Truncate(html, length, true, false);
        }
        public IHtmlString Truncate(string html, int length, bool addElipsis)
        {
            return Truncate(html, length, addElipsis, false);
        }
        public IHtmlString Truncate(string html, int length, bool addElipsis, bool treatTagsAsContent)
        {
        	return _umbracoHelper.Truncate(html, length, addElipsis, treatTagsAsContent);
        }


        public HtmlString StripHtml(IHtmlString html)
        {
			return _umbracoHelper.StripHtml(html);
        }
        public HtmlString StripHtml(DynamicNull html)
        {
            return new HtmlString(string.Empty);
        }
        public HtmlString StripHtml(string html)
        {
        	return _umbracoHelper.StripHtml(html);
        }

        public HtmlString StripHtml(IHtmlString html, List<string> tags)
        {
        	return _umbracoHelper.StripHtml(html, tags.ToArray());
        }
        public HtmlString StripHtml(DynamicNull html, List<string> tags)
        {
            return new HtmlString(string.Empty);
        }
        public HtmlString StripHtml(string html, List<string> tags)
        {
        	return _umbracoHelper.StripHtml(html, tags.ToArray());
        }

        public HtmlString StripHtml(IHtmlString html, params string[] tags)
        {
        	return _umbracoHelper.StripHtml(html, tags);
        }
        public HtmlString StripHtml(DynamicNull html, params string[] tags)
        {
            return new HtmlString(string.Empty);
        }
        public HtmlString StripHtml(string html, params string[] tags)
        {
        	return _umbracoHelper.StripHtml(html, tags);
        }

    }
}
