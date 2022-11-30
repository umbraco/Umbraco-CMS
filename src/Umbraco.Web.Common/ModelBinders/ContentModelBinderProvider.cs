using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Web.Common.ModelBinders;

/// <summary>
///     The provider for <see cref="ContentModelBinder" /> mapping view models, supporting mapping to and from any
///     IPublishedContent or IContentModel.
/// </summary>
public class ContentModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        Type modelType = context.Metadata.ModelType;

        // Can bind to ContentModel (exact type match)
        // or to ContentModel<TContent> (exact generic type match)
        // or to TContent where TContent : IPublishedContent (any IPublishedContent implementation)
        if (modelType == typeof(ContentModel) ||
            (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(ContentModel<>)) ||
            typeof(IPublishedContent).IsAssignableFrom(modelType))
        {
            return new BinderTypeModelBinder(typeof(ContentModelBinder));
        }

        return null;
    }
}
