using Umbraco.Cms.Api.Management.ViewModels.RelationType;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public class ObjectTypeViewModelFactory : IObjectTypeViewModelFactory
{
    public IEnumerable<ObjectTypeResponseModel> Create() =>
        new ObjectTypeResponseModel[]
        {
            new()
            {
                Id = UmbracoObjectTypes.Document.GetGuid(),
                Name = UmbracoObjectTypes.Document.GetFriendlyName(),
            },
            new()
            {
                Id = UmbracoObjectTypes.Media.GetGuid(),
                Name = UmbracoObjectTypes.Media.GetFriendlyName(),
            },
            new()
            {
                Id = UmbracoObjectTypes.Member.GetGuid(),
                Name = UmbracoObjectTypes.Member.GetFriendlyName(),
            },
            new()
            {
                Id = UmbracoObjectTypes.DocumentType.GetGuid(),
                Name = UmbracoObjectTypes.DocumentType.GetFriendlyName(),
            },
            new()
            {
                Id = UmbracoObjectTypes.MediaType.GetGuid(),
                Name = UmbracoObjectTypes.MediaType.GetFriendlyName(),
            },
            new()
            {
                Id = UmbracoObjectTypes.MemberType.GetGuid(),
                Name = UmbracoObjectTypes.MemberType.GetFriendlyName(),
            },
            new()
            {
                Id = UmbracoObjectTypes.DataType.GetGuid(),
                Name = UmbracoObjectTypes.DataType.GetFriendlyName(),
            },
            new()
            {
                Id = UmbracoObjectTypes.MemberGroup.GetGuid(),
                Name = UmbracoObjectTypes.MemberGroup.GetFriendlyName(),
            },
            new()
            {
                Id = UmbracoObjectTypes.ROOT.GetGuid(),
                Name = UmbracoObjectTypes.ROOT.GetFriendlyName(),
            },
            new()
            {
                Id = UmbracoObjectTypes.RecycleBin.GetGuid(),
                Name = UmbracoObjectTypes.RecycleBin.GetFriendlyName(),
            },
        };
}
