using System;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// Redirects to an Umbraco page by Id or Entity
	/// </summary>
	public class RedirectToUmbracoPageResult : ActionResult
	{
		private IDocument _document;
		private readonly int _pageId;
		private readonly UmbracoContext _umbracoContext;
		private string _url;
		public string Url
		{
			get
			{
				if (!_url.IsNullOrWhiteSpace()) return _url;

				if (Document == null)
				{
					throw new InvalidOperationException("Cannot redirect, no entity was found for id " + _pageId);
				}

				var result = _umbracoContext.RoutingContext.NiceUrlProvider.GetNiceUrl(Document.Id);
				if (result != NiceUrlProvider.NullUrl)
				{
					_url = result;
					return _url;
				}

				throw new InvalidOperationException("Could not route to entity with id " + _pageId + ", the NiceUrlProvider could not generate a URL");

			}
		}

		public IDocument Document
		{
			get
			{
				if (_document != null) return _document;

				//need to get the URL for the page
				_document = PublishedContentStoreResolver.Current.PublishedContentStore.GetDocumentById(_umbracoContext, _pageId);				

				return _document;
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
		/// <param name="document"></param>
		public RedirectToUmbracoPageResult(IDocument document)
			: this(document, UmbracoContext.Current)
		{
		}

		/// <summary>
		/// Creates a new RedirectToUmbracoResult
		/// </summary>
		/// <param name="document"></param>
		/// <param name="umbracoContext"></param>
		public RedirectToUmbracoPageResult(IDocument document, UmbracoContext umbracoContext)
		{
			_document = document;
			_pageId = document.Id;
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