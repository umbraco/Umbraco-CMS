using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Umbraco.Cms.ManagementApi.Filters;

public class UmbracoSystemTextJsonModelBinderConvention : IActionModelConvention
{
    private readonly IModelMetadataProvider _modelMetadataProvider;

    public UmbracoSystemTextJsonModelBinderConvention(IModelMetadataProvider modelMetadataProvider) => _modelMetadataProvider = modelMetadataProvider;

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
                        BinderType = typeof(UmbracoSystemTextJsonModelBinder),
                    };
                }

                continue;
            }

            if (p.BindingInfo.BindingSource == BindingSource.Body)
            {
                p.BindingInfo.BinderType = typeof(UmbracoSystemTextJsonModelBinder);
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
