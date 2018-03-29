using System;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Allows for Model Binding any IPublishedContent or IRenderModel
    /// </summary>
	public class RenderModelBinder : DefaultModelBinder, IModelBinderProvider
    {
        public static RenderModelBinder Instance = new RenderModelBinder();

		/// <summary>
		/// Binds the model to a value by using the specified controller context and binding context.
		/// </summary>
		/// <returns>
		/// The bound value.
		/// </returns>
		/// <param name="controllerContext">The controller context.</param><param name="bindingContext">The binding context.</param>
		public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
            object model;
            if (controllerContext.RouteData.DataTokens.TryGetValue(Core.Constants.Web.UmbracoDataToken, out model) == false)
                return null;

            //This model binder deals with IRenderModel and IPublishedContent by extracting the model from the route's
            // datatokens. This data token is set in 2 places: RenderRouteHandler, UmbracoVirtualNodeRouteHandler
            // and both always set the model to an instance of `RenderModel`. So if this isn't an instance of IRenderModel then
            // we need to let the DefaultModelBinder deal with the logic.
            var renderModel = model as IRenderModel;
            if (renderModel == null)
            {
                model = base.BindModel(controllerContext, bindingContext);
                if (model == null) return null;
            }

            //if for any reason the model is not either IRenderModel or IPublishedContent, then we return since those are the only
            // types this binder is dealing with.
		    if ((model is IRenderModel) == false && (model is IPublishedContent) == false) return null;

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

        public class ModelBindingArgs : EventArgs
        {
            public ModelBindingArgs(Type sourceType, Type modelType, StringBuilder message)
            {
                SourceType = sourceType;
                ModelType = modelType;
                Message = message;
            }

            public bool SourceIsContent;
            public bool ViewModelIsContent;
            public Type SourceType { get; set; }
            public Type ModelType { get; set; }
            public StringBuilder Message { get; private set; }
            public bool Restart { get; set; }
        }

        public static event EventHandler<ModelBindingArgs> ModelBindingException;

	    private static void ThrowModelBindingException(bool sourceContent, bool modelContent, Type sourceType, Type modelType)
	    {
	        var msg = new StringBuilder();

            // prepare message
	        msg.Append("Cannot bind source");
	        if (sourceContent) msg.Append(" content");
	        msg.Append(" type ");
	        msg.Append(sourceType.FullName);
	        msg.Append(" to model");
	        if (modelContent) msg.Append(" content");
	        msg.Append(" type ");
            msg.Append(modelType.FullName);
            msg.Append(".");

            // raise event, to give model factories a chance at reporting
            // the error with more details, and optionally request that
            // the application restarts.

	        var args = new ModelBindingArgs(sourceType, modelType, msg);
            if (ModelBindingException != null)
                ModelBindingException(Instance, args);

	        if (args.Restart)
	        {
	            msg.Append(" The application is restarting now.");

	            var context = HttpContext.Current;
	            if (context == null)
	                AppDomain.Unload(AppDomain.CurrentDomain);
	            else
	                ApplicationContext.Current.RestartApplicationPool(new HttpContextWrapper(context));
	        }

	        throw new ModelBindingException(msg.ToString());
	    }

        public IModelBinder GetBinder(Type modelType)
        {
            // can bind to RenderModel (exact type match)
            // You might be tempted to change this to IRenderModel but do not change this: http://issues.umbraco.org/issue/U4-8216
            if (modelType == typeof(RenderModel)) return this;

            // can bind to RenderModel<TContent> (exact generic type match)
            if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(RenderModel<>)) return this;

            // can bind to TContent where TContent : IPublishedContent (any IPublishedContent implementation)
            if (typeof(IPublishedContent).IsAssignableFrom(modelType)) return this;
            return null;
        }
    }
}