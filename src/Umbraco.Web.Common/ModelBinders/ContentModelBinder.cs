using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Models;

namespace Umbraco.Web.Common.ModelBinders
{
    /// <summary>
    /// Maps view models, supporting mapping to and from any IPublishedContent or IContentModel.
    /// </summary>
    public class ContentModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.ActionContext.RouteData.DataTokens.TryGetValue(Core.Constants.Web.UmbracoDataToken, out var source) == false)
            {
                return Task.CompletedTask;
            }

            // This model binder deals with IContentModel and IPublishedContent by extracting the model from the route's
            // datatokens. This data token is set in 2 places: RenderRouteHandler, UmbracoVirtualNodeRouteHandler
            // and both always set the model to an instance of `ContentModel`.

            // No need for type checks to ensure we have the appropriate binder, as in .NET Core this is handled in the provider,
            // in this case ContentModelBinderProvider.

            // Being defensive though.... if for any reason the model is not either IContentModel or IPublishedContent,
            // then we return since those are the only types this binder is dealing with.
            if (source is IContentModel == false && source is IPublishedContent == false)
            {
                return Task.CompletedTask;
            }

            BindModelAsync(bindingContext, source, bindingContext.ModelType);
            return Task.CompletedTask;
        }

        // source is the model that we have
        // modelType is the type of the model that we need to bind to
        //
        // create a model object of the modelType by mapping:
        // { ContentModel, ContentModel<TContent>, IPublishedContent }
        // to
        // { ContentModel, ContentModel<TContent>, IPublishedContent }
        //
        public Task BindModelAsync(ModelBindingContext bindingContext, object source, Type modelType)
        {
            // Null model, return
            if (source == null)
            {
                return Task.CompletedTask;
            }

            // If types already match, return
            var sourceType = source.GetType();
            if (sourceType.   Inherits(modelType)) // includes ==
            {
                bindingContext.Result = ModelBindingResult.Success(source);
                return Task.CompletedTask;
            }

            // Try to grab the content
            var sourceContent = source as IPublishedContent; // check if what we have is an IPublishedContent
            if (sourceContent == null && sourceType.Implements<IContentModel>())
                // else check if it's an IContentModel, and get the content
                sourceContent = ((IContentModel)source).Content;
            if (sourceContent == null)
            {
                // else check if we can convert it to a content
                var attempt1 = source.TryConvertTo<IPublishedContent>();
                if (attempt1.Success) sourceContent = attempt1.Result;
            }

            // If we have a content
            if (sourceContent != null)
            {
                // If model is IPublishedContent, check content type and return
                if (modelType.Implements<IPublishedContent>())
                {
                    if (sourceContent.GetType().Inherits(modelType) == false)
                    {
                        ThrowModelBindingException(true, false, sourceContent.GetType(), modelType);
                    }

                    bindingContext.Result = ModelBindingResult.Success(sourceContent);
                    return Task.CompletedTask;
                }

                // If model is ContentModel, create and return
                if (modelType == typeof(ContentModel))
                {
                    bindingContext.Result = ModelBindingResult.Success(new ContentModel(sourceContent));
                    return Task.CompletedTask;
                }

                // If model is ContentModel<TContent>, check content type, then create and return
                if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(ContentModel<>))
                {
                    var targetContentType = modelType.GetGenericArguments()[0];
                    if (sourceContent.GetType().Inherits(targetContentType) == false)
                    {
                        ThrowModelBindingException(true, true, sourceContent.GetType(), targetContentType);
                    }

                    bindingContext.Result = ModelBindingResult.Success(Activator.CreateInstance(modelType, sourceContent));
                    return Task.CompletedTask;
                }
            }

            // Last chance : try to convert
            var attempt2 = source.TryConvertTo(modelType);
            if (attempt2.Success)
            {
                bindingContext.Result = ModelBindingResult.Success(attempt2.Result);
                return Task.CompletedTask;
            }

            // Fail
            ThrowModelBindingException(false, false, sourceType, modelType);
            return Task.CompletedTask;
        }

        private void ThrowModelBindingException(bool sourceContent, bool modelContent, Type sourceType, Type modelType)
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
            ModelBindingException?.Invoke(this, args);

            throw new ModelBindingException(msg.ToString());
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
