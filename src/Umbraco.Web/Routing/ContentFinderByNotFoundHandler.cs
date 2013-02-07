using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;

using umbraco;
using umbraco.interfaces;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides an implementation of <see cref="IContentFinder"/> that runs a legacy NotFoundHandler.
	/// </summary>
	/// <remarks>Provided for backward compatibility.</remarks>
	internal class ContentFinderByNotFoundHandler<Thandler> : IContentFinder
	{
		/// <summary>
		/// Tries to find and assign an Umbraco document to a <c>PublishedContentRequest</c>.
		/// </summary>
		/// <param name="pcr">The <c>PublishedContentRequest</c>.</param>		
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		public bool TryFindDocument(PublishedContentRequest pcr)
		{
			var type = typeof(Thandler);
			var handler = GetHandler(type);
			
			if (handler == null)
				return false;

			var url = NotFoundHandlerHelper.GetLegacyUrlForNotFoundHandlers();
            LogHelper.Debug<ContentFinderByNotFoundHandler<Thandler>>("Running for legacy url='{0}'.", () => url);

			if (handler.Execute(url) && handler.redirectID > 0)
			{
				LogHelper.Debug<ContentFinderByNotFoundHandler<Thandler>>("Handler '{0}' returned id={1}.", () => type.FullName, () => handler.redirectID);

				var content = pcr.RoutingContext.PublishedContentStore.GetDocumentById(
						pcr.RoutingContext.UmbracoContext,
						handler.redirectID);

				if (content == null)
					LogHelper.Debug<ContentFinderByNotFoundHandler<Thandler>>("Could not find content with that id.");
				else
					LogHelper.Debug<ContentFinderByNotFoundHandler<Thandler>>("Found corresponding content.");

				pcr.PublishedContent = content;
				return content != null;
			}
			else
			{
				LogHelper.Debug<ContentFinderByNotFoundHandler<Thandler>>("Handler '{0}' returned nothing.", () => type.FullName);
				return false;
			}
		}

		INotFoundHandler GetHandler(Type type)
		{
			try
			{
				return Activator.CreateInstance(type) as INotFoundHandler;
			}
			catch (Exception e)
			{
				LogHelper.Error<ContentFinderByNotFoundHandler<Thandler>>(string.Format("Error instanciating handler {0}, ignoring.", type.FullName), e);
				return null;
			}
		}
	}
}
