using Microsoft.AspNetCore.Mvc.ModelBinding;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.BackOfficeApi.ModelBinding;

public class InstallMetadataBinderProvider : IModelBinderProvider
{
    private readonly IContentService _contentService;

    public InstallMetadataBinderProvider(IContentService contentService)
    {
        _contentService = contentService;
    }

    public IModelBinder? GetBinder(ModelBinderProviderContext context) => throw new NotImplementedException();
}
