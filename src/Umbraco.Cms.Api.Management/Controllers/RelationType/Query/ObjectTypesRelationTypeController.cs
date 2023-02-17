using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.RelationType;
using Umbraco.Cms.Api.Management.ViewModels.Telemetry;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.RelationType.Query;

public class ObjectTypesRelationTypeController : RelationTypeControllerBase
{
    [HttpGet("object-types")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(TelemetryViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObjectTypes(int skip = 0, int take = 100)
    {
        var objectTypes = new List<ObjectTypeViewModel>
        {
            new()
            {
                Id = UmbracoObjectTypes.Document.GetGuid(), Name = UmbracoObjectTypes.Document.GetFriendlyName()
            },
            new() {Id = UmbracoObjectTypes.Media.GetGuid(), Name = UmbracoObjectTypes.Media.GetFriendlyName()},
            new() {Id = UmbracoObjectTypes.Member.GetGuid(), Name = UmbracoObjectTypes.Member.GetFriendlyName()},
            new()
            {
                Id = UmbracoObjectTypes.DocumentType.GetGuid(),
                Name = UmbracoObjectTypes.DocumentType.GetFriendlyName()
            },
            new()
            {
                Id = UmbracoObjectTypes.MediaType.GetGuid(),
                Name = UmbracoObjectTypes.MediaType.GetFriendlyName()
            },
            new()
            {
                Id = UmbracoObjectTypes.MemberType.GetGuid(),
                Name = UmbracoObjectTypes.MemberType.GetFriendlyName()
            },
            new()
            {
                Id = UmbracoObjectTypes.DataType.GetGuid(), Name = UmbracoObjectTypes.DataType.GetFriendlyName()
            },
            new()
            {
                Id = UmbracoObjectTypes.MemberGroup.GetGuid(),
                Name = UmbracoObjectTypes.MemberGroup.GetFriendlyName()
            },
            new() {Id = UmbracoObjectTypes.ROOT.GetGuid(), Name = UmbracoObjectTypes.ROOT.GetFriendlyName()},
            new()
            {
                Id = UmbracoObjectTypes.RecycleBin.GetGuid(),
                Name = UmbracoObjectTypes.RecycleBin.GetFriendlyName()
            }
        };

        return await Task.FromResult(Ok(objectTypes.Skip(skip).Take(take)));
    }
}
