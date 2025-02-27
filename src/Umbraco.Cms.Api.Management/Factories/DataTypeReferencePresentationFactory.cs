using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class DataTypeReferencePresentationFactory : IDataTypeReferencePresentationFactory
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IMemberTypeService _memberTypeService;

    public DataTypeReferencePresentationFactory(
        IContentTypeService contentTypeService,
        IMediaTypeService mediaTypeService,
        IMemberTypeService memberTypeService)
    {
        _contentTypeService = contentTypeService;
        _mediaTypeService = mediaTypeService;
        _memberTypeService = memberTypeService;
    }

    public IEnumerable<DataTypeReferenceResponseModel> CreateDataTypeReferenceViewModels(IReadOnlyDictionary<Udi, IEnumerable<string>> dataTypeUsages)
    {
        var getContentTypesByObjectType = new Dictionary<string, Func<IEnumerable<Guid>, IEnumerable<IContentTypeBase>>>
        {
            { UmbracoObjectTypes.DocumentType.GetUdiType(), keys => _contentTypeService.GetMany(keys) },
            { UmbracoObjectTypes.MediaType.GetUdiType(), keys => _mediaTypeService.GetMany(keys) },
            { UmbracoObjectTypes.MemberType.GetUdiType(), keys => _memberTypeService.GetMany(keys) }
        };

        foreach (IGrouping<string, KeyValuePair<Udi, IEnumerable<string>>> usagesByEntityType in dataTypeUsages.GroupBy(u => u.Key.EntityType))
        {
            if (getContentTypesByObjectType.TryGetValue(usagesByEntityType.Key, out Func<IEnumerable<Guid>, IEnumerable<IContentTypeBase>>? getContentTypes) == false)
            {
                continue;
            }

            var propertyAliasesByGuid = usagesByEntityType.ToDictionary(i => ((GuidUdi)i.Key).Guid, i => i.Value);

            IContentTypeBase[] contentTypes = getContentTypes(propertyAliasesByGuid.Keys).ToArray();

            foreach (IContentTypeBase contentType in contentTypes)
            {
                IEnumerable<string> propertyAliases = propertyAliasesByGuid[contentType.Key];
                yield return new DataTypeReferenceResponseModel
                {
                    ContentType = new DataTypeContentTypeReferenceModel
                    {
                        Id = contentType.Key,
                        Name = contentType.Name,
                        Icon = contentType.Icon,
                        Type = usagesByEntityType.Key,
                    },
                    Properties = contentType
                        .PropertyTypes
                        .Where(propertyType => propertyAliases.InvariantContains(propertyType.Alias))
                        .Select(propertyType => new DataTypePropertyReferenceViewModel
                        {
                            Name = propertyType.Name,
                            Alias = propertyType.Alias
                        })
                };
            }
        }
    }
}
