using System;
using Umbraco.Core.Logging;
using umbraco.interfaces;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides an implementation of <see cref="IContentFinder"/> that runs a legacy NotFoundHandler.
	/// </summary>
	/// <remarks>Provided for backward compatibility.</remarks>
	public class ContentFinderByNotFoundHandler<THandler> : IContentFinder
	{
		/// <summary>
		/// Tries to find and assign an Umbraco document to a <c>PublishedContentRequest</c>.
		/// </summary>
		/// <param name="pcr">The <c>PublishedContentRequest</c>.</param>		
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		public bool TryFindContent(PublishedContentRequest pcr)
		{
			var type = typeof(THandler);
			var handler = GetHandler(type);
			
			if (handler == null)
				return false;

			var url = NotFoundHandlerHelper.GetLegacyUrlForNotFoundHandlers();
            LogHelper.Debug<ContentFinderByNotFoundHandler<THandler>>("Running for legacy url='{0}'.", () => url);

			if (handler.Execute(url) && handler.redirectID > 0)
			{
				LogHelper.Debug<ContentFinderByNotFoundHandler<THandler>>("Handler '{0}' returned id={1}.", () => type.FullName, () => handler.redirectID);

                var content = pcr.RoutingContext.UmbracoContext.ContentCache.GetById(handler.redirectID);

			    LogHelper.Debug<ContentFinderByNotFoundHandler<THandler>>(content == null
			        ? "Could not find content with that id."
			        : "Found corresponding content.");

			    pcr.PublishedContent = content;
				return content != null;
			}
		    
            LogHelper.Debug<ContentFinderByNotFoundHandler<THandler>>("Handler '{0}' returned nothing.", () => type.FullName);
		    return false;
		}

		INotFoundHandler GetHandler(Type type)
		{
			try
			{
				return Activator.CreateInstance(type) as INotFoundHandler;
			}
			catch (Exception e)
			{
				LogHelper.Error<ContentFinderByNotFoundHandler<THandler>>(string.Format("Error instanciating handler {0}, ignoring.", type.FullName), e);
				return null;
			}
		}
	}
}
