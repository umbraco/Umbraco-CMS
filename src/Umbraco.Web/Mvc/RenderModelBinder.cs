using System;
using System.Globalization;
using System.Text;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models;

namespace Umbraco.Web.Mvc
{
	public class RenderModelBinder : IModelBinder, IModelBinderProvider
    {
		/// <summary>
		/// Binds the model to a value by using the specified controller context and binding context.
		/// </summary>
		/// <returns>
		/// The bound value.
		/// </returns>
		/// <param name="controllerContext">The controller context.</param><param name="bindingContext">The binding context.</param>
		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
            object model;
            if (controllerContext.RouteData.DataTokens.TryGetValue(Core.Constants.Web.UmbracoDataToken, out model) == false)
                return null;

            // when rendering "special" stuff such as surface controllers, etc, the token does *not* contain
            // the model source, but some special strings - and then we have to find the source using the
            // "default" MVC way
            var modelString = model as string;
            if (modelString == "surface" || modelString == "api" || modelString == "backoffice") // fixme - more?
            {
                var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
                model = value.RawValue;

                // fixme - should we return here?
                // or go with the binding logic below that's nicer with strongly typed models
            }

            //default culture
            var culture = CultureInfo.CurrentCulture;

		    var umbracoContext = controllerContext.GetUmbracoContext()
		                         ?? UmbracoContext.Current;

		    if (umbracoContext != null && umbracoContext.PublishedContentRequest != null)
		    {
		        culture = umbracoContext.PublishedContentRequest.Culture;
		    }

            return BindModel(model, bindingContext.ModelType, culture);
        }

        // source is the model that we have
        // modelType is the type of the model that we need to bind to
        // culture is the CultureInfo that we have, used by RenderModel
        //
        // create a model object of the modelType by mapping:
        // { RenderModel, RenderModel<TContent>, IPublishedContent }
        // to
        // { RenderModel, RenderModel<TContent>, IPublishedContent }
        //
        public static object BindModel(object source, Type modelType, CultureInfo culture)
        {
            // null model, return
            if (source == null) return null;

            // if types already match, return
            var sourceType = source.GetType();
            if (sourceType.Inherits(modelType)) // includes ==
                return source;

            // try to grab the content
            var sourceContent = source as IPublishedContent; // check if what we have is an IPublishedContent
            if (sourceContent == null && sourceType.Implements<IRenderModel>())
            {
                // else check if it's an IRenderModel, and get the content
                sourceContent = ((IRenderModel)source).Content;
            }
            if (sourceContent == null)
            {
                // else check if we can convert it to a content
                var attempt1 = source.TryConvertTo<IPublishedContent>();
                if (attempt1.Success) sourceContent = attempt1.Result;
            }

            // if we have a content
            if (sourceContent != null)
            {
                // try to grab the culture
                // using supplied culture by default
                var sourceRenderModel = source as RenderModel;
                if (sourceRenderModel != null)
                    culture = sourceRenderModel.CurrentCulture;

                // if model is IPublishedContent, check content type and return
                if (modelType.Implements<IPublishedContent>())
                {
                    if ((sourceContent.GetType().Inherits(modelType)) == false)
                        ThrowModelBindingException(true, false, sourceContent.GetType(), modelType);
                    return sourceContent;
                }

                // if model is RenderModel, create and return
                if (modelType == typeof(RenderModel))
                {
                    return new RenderModel(sourceContent, culture);
                }

                // if model is RenderModel<TContent>, check content type, then create and return
                if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(RenderModel<>))
                {
                    var targetContentType = modelType.GetGenericArguments()[0];
                    if ((sourceContent.GetType().Inherits(targetContentType)) == false)
                        ThrowModelBindingException(true, true, sourceContent.GetType(), targetContentType);
                    return Activator.CreateInstance(modelType, sourceContent, culture);
                }
            }

            // last chance : try to convert
            var attempt2 = source.TryConvertTo(modelType);
            if (attempt2.Success) return attempt2.Result;

            // fail
            ThrowModelBindingException(false, false, sourceType, modelType);
            return null;
        }

	    private static void ThrowModelBindingException(bool sourceContent, bool modelContent, Type sourceType, Type modelType)
	    {
	        var msg = new StringBuilder();

	        msg.Append("Cannot bind source");
	        if (sourceContent) msg.Append(" content");
	        msg.Append(" type ");
	        msg.Append(sourceType.FullName);
	        msg.Append(" to model");
	        if (modelContent) msg.Append(" content");
	        msg.Append(" type");

	        if (sourceType.FullName == modelType.FullName)
	        {
	            msg.Append(". Same type name but different assemblies.");
	        }
	        else
	        {
	            msg.Append(" ");
                msg.Append(modelType.FullName);
                msg.Append(".");
            }

	        throw new ModelBindingException(msg.ToString());
	    }

        public IModelBinder GetBinder(Type modelType)
        {
            // can bind to RenderModel
            if (modelType == typeof(RenderModel)) return this;

            // can bind to RenderModel<TContent>
            if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(RenderModel<>)) return this;

            // can bind to TContent where TContent : IPublishedContent
            if (typeof(IPublishedContent).IsAssignableFrom(modelType)) return this;
            return null;
        }
    }
}