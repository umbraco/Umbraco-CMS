using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Manifest;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Manifest;

[VersionedApiBackOfficeRoute("manifest")]
[ApiExplorerSettings(GroupName = "Manifest")]
public abstract class ManifestControllerBase : ManagementApiControllerBase
{
    protected static void ReplaceCacheBusterTokens(
        IEnumerable<ManifestResponseModel> models, string cacheBustHash)
    {
        foreach (ManifestResponseModel model in models)
        {
            if (model.Extensions.Length == 0)
            {
                continue;
            }

            var json = JsonSerializer.Serialize(model.Extensions);
            if (json.Contains(Constants.Web.CacheBusterToken) is false)
            {
                continue;
            }

            json = json.Replace(Constants.Web.CacheBusterToken, JsonEncodedText.Encode(cacheBustHash).ToString());
            model.Extensions = JsonSerializer.Deserialize<object[]>(json) ?? model.Extensions;
        }
    }
}
