using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Cms.Api.Management.Headers;

public class MultiResourceListPatch : AddHeaderAttribute
{
    public MultiResourceListPatch()
        : base(
            "Accept-Patch",
            "application/umbraco.com.api.MultiResourceList+json")
    {
    }
}
