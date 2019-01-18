using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Composing;
using Umbraco.Web.Models;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Maps view models, supporting mapping to and from any IPublishedContent or IContentModel.
    /// </summary>
    public class ContentModelBinder : DefaultModelBinder, IModelBinderProvider
    {
        // use Instance
        private ContentModelBinder() { }

        public static ContentModelBinder Instance = new ContentModelBinder();

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

            // this model binder deals with IContentModel and IPublishedContent by extracting the model from the route's
            // datatokens. This data token is set in 2 places: RenderRouteHandler, UmbracoVirtualNodeRouteHandler
            // and both always set the model to an instance of `ContentMOdel`. So if this isn't an instance of IContentModel then
            // we need to let the DefaultModelBinder deal with the logic.
            var contentModel = model as IContentModel;
            if (contentModel == null)
            {
                model = base.BindModel(controllerContext, bindingContext);
                if (model == null) return null;
            }

            //if for any reason the model is not either IContentModel or IPublishedContent, then we return since those are the only
            // types this binder is dealing with.
            if ((model is IContentModel) == false && (model is IPublishedContent) == false) return null;

            return BindModel(model, bindingContext.ModelType);
        }

        // source is the model that we have
        // modelType is the type of the model that we need to bind to
        //
        // create a model object of the modelType by mapping:
        // { ContentModel, ContentModel<TContent>, IPublishedContent }
        // to
        // { ContentModel, ContentModel<TContent>, IPublishedContent }
        //
        public static object BindModel(object source, Type modelType)
        {
            // null model, return
            if (source == null) return null;

            // if types already match, return
            var sourceType = source.GetType();
            if (sourceType.Inherits(modelType)) // includes ==
                return source;

            // try to grab the content
            var sourceContent = source as IPublishedContent; // check if what we have is an IPublishedContent
            if (sourceContent == null && sourceType.Implements<IContentModel>())
            {
                // else check if it's an IContentModel, and get the content
                sourceContent = ((IContentModel)source).Content;
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
                // if model is IPublishedContent, check content type and return
                if (modelType.Implements<IPublishedContent>())
                {
                    if ((sourceContent.GetType().Inherits(modelType)) == false)
                        ThrowModelBindingException(true, false, sourceContent.GetType(), modelType);
                    return sourceContent;
                }

                // if model is ContentModel, create and return
                if (modelType == typeof(ContentModel))
                {
                    return new ContentModel(sourceContent);
                }

                // if model is ContentModel<TContent>, check content type, then create and return
                if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(ContentModel<>))
                {
                    var targetContentType = modelType.GetGenericArguments()[0];
                    if ((sourceContent.GetType().Inherits(targetContentType)) == false)
                        ThrowModelBindingException(true, true, sourceContent.GetType(), targetContentType);
                    return Activator.CreateInstance(modelType, sourceContent);
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
            ModelBindingException?.Invoke(Instance, args);

            if (args.Restart)
            {
                msg.Append(" The application is restarting now.");

                var context = HttpContext.Current;
                if (context == null)
                    AppDomain.Unload(AppDomain.CurrentDomain);
                else
                    UmbracoApplication.Restart(new HttpContextWrapper(context));
            }

            throw new ModelBindingException(msg.ToString());
        }

        public IModelBinder GetBinder(Type modelType)
        {
            // can bind to ContentModel (exact type match)
            if (modelType == typeof(ContentModel)) return this;

            // can bind to ContentModel<TContent> (exact generic type match)
            if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(ContentModel<>)) return this;

            // can bind to TContent where TContent : IPublishedContent (any IPublishedContent implementation)
            if (typeof(IPublishedContent).IsAssignableFrom(modelType)) return this;
            return null;
        }

        /// <summary>
        /// Contains event data for the <see cref="ModelBindingException"/> event.
        /// </summary>
        public class ModelBindingArgs : EventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ModelBindingArgs"/> class.
            /// </summary>
            public ModelBindingArgs(Type sourceType, Type modelType, StringBuilder message)
            {
                SourceType = sourceType;
                ModelType = modelType;
                Message = message;
            }

            /// <summary>
            /// Gets the type of the source object.
            /// </summary>
            public Type SourceType { get; set; }

            /// <summary>
            /// Gets the type of the view model.
            /// </summary>
            public Type ModelType { get; set; }

            /// <summary>
            /// Gets the message string builder.
            /// </summary>
            /// <remarks>Handlers of the event can append text to the message.</remarks>
            public StringBuilder Message { get; }

            /// <summary>
            /// Gets or sets a value indicating whether the application should restart.
            /// </summary>
            public bool Restart { get; set; }
        }

        /// <summary>
        /// Occurs on model binding exceptions.
        /// </summary>
        public static event EventHandler<ModelBindingArgs> ModelBindingException;
    }
}
