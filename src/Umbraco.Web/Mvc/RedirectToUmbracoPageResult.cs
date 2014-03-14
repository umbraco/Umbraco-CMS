using System;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Mvc
{
    /// <summary>
	/// Redirects to an Umbraco page by Id or Entity
	/// </summary>
	public class RedirectToUmbracoPageResult : ActionResult
	{
		private IPublishedContent _publishedContent;
		private readonly int _pageId;
		private readonly UmbracoContext _umbracoContext;
		private string _url;
		public string Url
		{
			get
			{
				if (!_url.IsNullOrWhiteSpace()) return _url;

				if (PublishedContent == null)
				{
					throw new InvalidOperationException("Cannot redirect, no entity was found for id " + _pageId);
				}

				var result = _umbracoContext.RoutingContext.UrlProvider.GetUrl(PublishedContent.Id);
				if (result != "#")
				{
					_url = result;
					return _url;
				}

				throw new InvalidOperationException("Could not route to entity with id " + _pageId + ", the NiceUrlProvider could not generate a URL");

			}
		}

		public IPublishedContent PublishedContent
		{
			get
			{
				if (_publishedContent != null) return _publishedContent;

				//need to get the URL for the page
			    _publishedContent = UmbracoContext.Current.ContentCache.GetById(_pageId);

				return _publishedContent;
			}
		}

		/// <summary>
		/// Creates a new RedirectToUmbracoResult
		/// </summary>
		/// <param name="pageId"></param>
		public RedirectToUmbracoPageResult(int pageId)
			: this(pageId, UmbracoContext.Current)
		{
		}

		/// <summary>
		/// Creates a new RedirectToUmbracoResult
		/// </summary>
		/// <param name="publishedContent"></param>
		public RedirectToUmbracoPageResult(IPublishedContent publishedContent)
			: this(publishedContent, UmbracoContext.Current)
		{
		}

		/// <summary>
		/// Creates a new RedirectToUmbracoResult
		/// </summary>
		/// <param name="publishedContent"></param>
		/// <param name="umbracoContext"></param>
		public RedirectToUmbracoPageResult(IPublishedContent publishedContent, UmbracoContext umbracoContext)
		{
			_publishedContent = publishedContent;
			_pageId = publishedContent.Id;
			_umbracoContext = umbracoContext;
		}

		/// <summary>
		/// Creates a new RedirectToUmbracoResult
		/// </summary>
		/// <param name="pageId"></param>
		/// <param name="umbracoContext"></param>
		public RedirectToUmbracoPageResult(int pageId, UmbracoContext umbracoContext)
		{
			_pageId = pageId;
			_umbracoContext = umbracoContext;
		}

		public override void ExecuteResult(ControllerContext context)
		{
			if (context == null) throw new ArgumentNullException("context");

			if (context.IsChildAction)
			{
				throw new InvalidOperationException("Cannot redirect from a Child Action");
			}

			var destinationUrl = UrlHelper.GenerateContentUrl(Url, context.HttpContext);
			context.Controller.TempData.Keep();

			context.HttpContext.Response.Redirect(destinationUrl, endResponse: false);
		}

	}
}