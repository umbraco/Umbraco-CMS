using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Cms.Web.Common.ModelBinders;

namespace Umbraco.Cms.Web.Common.ApplicationModels;

/// <summary>
///     Applies the <see cref="UmbracoJsonModelBinder" /> body model binder to any complex parameter and those with a
///     binding source of type <see cref="BindingSource.Body" />
/// </summary>
public class UmbracoJsonModelBinderConvention : IActionModelConvention
{
    private readonly IModelMetadataProvider _modelMetadataProvider;

    public UmbracoJsonModelBinderConvention()
        : this(StaticServiceProvider.Instance.GetRequiredService<IModelMetadataProvider>())
    {
    }

    public UmbracoJsonModelBinderConvention(IModelMetadataProvider modelMetadataProvider) =>
        _modelMetadataProvider = modelMetadataProvider;

    /// <inheritdoc />
    public void Apply(ActionModel action)
    {
        foreach (ParameterModel p in action.Parameters)
        {
            if (p.BindingInfo == null)
            {
                if (IsComplexTypeParameter(p))
                {
                    p.BindingInfo = new BindingInfo
                    {
                        BindingSource = BindingSource.Body,
                        BinderType = typeof(UmbracoJsonModelBinder),
                    };
                }

                continue;
            }

            if (p.BindingInfo.BindingSource == BindingSource.Body)
            {
                p.BindingInfo.BinderType = typeof(UmbracoJsonModelBinder);
            }
        }
    }

    private bool IsComplexTypeParameter(ParameterModel parameter)
    {
        // No need for information from attributes on the parameter. Just use its type.
        ModelMetadata metadata = _modelMetadataProvider.GetMetadataForType(parameter.ParameterInfo.ParameterType);

        return metadata.IsComplexType;
    }
}
