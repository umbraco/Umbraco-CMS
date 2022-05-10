using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.ModelBinders;

/// <summary>
///     Maps view models, supporting mapping to and from any <see cref="IPublishedContent" /> or
///     <see cref="IContentModel" />.
/// </summary>
public class ContentModelBinder : IModelBinder
{
    private readonly IEventAggregator _eventAggregator;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentModelBinder" /> class.
    /// </summary>
    public ContentModelBinder(IEventAggregator eventAggregator) => _eventAggregator = eventAggregator;

    /// <inheritdoc />
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        // Although this model binder is built to work both ways between IPublishedContent and IContentModel in reality
        // only IPublishedContent will ever exist in the request so when this model binder is used as an IModelBinder
        // in the aspnet pipeline it will really only support converting from IPublishedContent which is contained
        // in the UmbracoRouteValues --> IContentModel
        UmbracoRouteValues? umbracoRouteValues = bindingContext.HttpContext.Features.Get<UmbracoRouteValues>();
        if (umbracoRouteValues is null)
        {
            return Task.CompletedTask;
        }

        BindModel(bindingContext, umbracoRouteValues.PublishedRequest.PublishedContent, bindingContext.ModelType);
        return Task.CompletedTask;
    }

    // source is the model that we have
    // modelType is the type of the model that we need to bind to
    //
    // create a model object of the modelType by mapping:
    // { ContentModel, ContentModel<TContent>, IPublishedContent }
    // to
    // { ContentModel, ContentModel<TContent>, IPublishedContent }

    /// <summary>
    ///     Attempts to bind the model
    /// </summary>
    public void BindModel(ModelBindingContext bindingContext, object? source, Type modelType)
    {
        // Null model, return
        if (source == null)
        {
            return;
        }

        // If types already match, return
        Type sourceType = source.GetType();
        if (sourceType.Inherits(modelType))
        {
            bindingContext.Result = ModelBindingResult.Success(source);
            return;
        }

        // Try to grab the content
        var sourceContent = source as IPublishedContent; // check if what we have is an IPublishedContent
        if (sourceContent == null && sourceType.Implements<IContentModel>())
        {
            // else check if it's an IContentModel, and get the content
            sourceContent = ((IContentModel)source).Content;
        }

        if (sourceContent == null)
        {
            // else check if we can convert it to a content
            Attempt<IPublishedContent> attempt1 = source.TryConvertTo<IPublishedContent>();
            if (attempt1.Success)
            {
                sourceContent = attempt1.Result;
            }
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
                return;
            }

            // If model is ContentModel, create and return
            if (modelType == typeof(ContentModel))
            {
                bindingContext.Result = ModelBindingResult.Success(new ContentModel(sourceContent));
                return;
            }

            // If model is ContentModel<TContent>, check content type, then create and return
            if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(ContentModel<>))
            {
                Type targetContentType = modelType.GetGenericArguments()[0];
                if (sourceContent.GetType().Inherits(targetContentType) == false)
                {
                    ThrowModelBindingException(true, true, sourceContent.GetType(), targetContentType);
                }

                bindingContext.Result = ModelBindingResult.Success(Activator.CreateInstance(modelType, sourceContent));
                return;
            }
        }

        // Last chance : try to convert
        Attempt<object?> attempt2 = source.TryConvertTo(modelType);
        if (attempt2.Success)
        {
            bindingContext.Result = ModelBindingResult.Success(attempt2.Result);
            return;
        }

        // Fail
        ThrowModelBindingException(false, false, sourceType, modelType);
    }

    private void ThrowModelBindingException(bool sourceContent, bool modelContent, Type sourceType, Type modelType)
    {
        var msg = new StringBuilder();

        // prepare message
        msg.Append("Cannot bind source");
        if (sourceContent)
        {
            msg.Append(" content");
        }

        msg.Append(" type ");
        msg.Append(sourceType.FullName);
        msg.Append(" to model");
        if (modelContent)
        {
            msg.Append(" content");
        }

        msg.Append(" type ");
        msg.Append(modelType.FullName);
        msg.Append(".");

        // raise event, to give model factories a chance at reporting
        // the error with more details, and optionally request that
        // the application restarts.
        var args = new ModelBindingErrorNotification(sourceType, modelType, msg);
        _eventAggregator.Publish(args);

        throw new ModelBindingException(msg.ToString());
    }
}
