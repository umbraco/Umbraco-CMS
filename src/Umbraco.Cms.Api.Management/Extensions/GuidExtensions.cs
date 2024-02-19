using Umbraco.Cms.Api.Management.ViewModels;

namespace Umbraco.Cms.Api.Management.Extensions;

public static class ListViewExtensions
{
    public static ReferenceByIdModel? ToReferenceByIdModel(this Guid? guid)
    {
        if (guid is not null)
        {
            return new ReferenceByIdModel
            {
                Id = guid.Value,
            };
        }

        return null;
    }
}
