using System.Globalization;
using NPoco.FluentMappings;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Search;
using Umbraco.Cms.Core.Search;
using Umbraco.Extensions;

namespace Umbraco.Search;

public class SearchMapper : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IUmbracoSearchResult, SearchResultEntity>((source, context) => new SearchResultEntity(), Map);
        mapper.Define<IUmbracoSearchResults, IEnumerable<SearchResultEntity>>((source, context) =>
            context.MapEnumerable<IUmbracoSearchResult, SearchResultEntity>(source).WhereNotNull());
        mapper.Define<IEnumerable<IUmbracoSearchResult>, IEnumerable<SearchResultEntity>>((source, context) =>
            context.MapEnumerable<IUmbracoSearchResult, SearchResultEntity>(source).WhereNotNull());
    }
    // Umbraco.Code.MapAll -Alias -Trashed
    private static void Map(IUmbracoSearchResult source, SearchResultEntity target, MapperContext context)
    {
        target.Id = source.Id;
        target.Score = source.Score;

        // TODO: Properly map this (not aftermap)

        // get the icon if there is one
        target.Icon = source.Values.ContainsKey(UmbracoSearchFieldNames.IconFieldName)
            ? source.Values[UmbracoSearchFieldNames.IconFieldName]
            : Constants.Icons.DefaultIcon;

        target.Name = source.Values.ContainsKey(UmbracoSearchFieldNames.NodeNameFieldName)
            ? source.Values[UmbracoSearchFieldNames.NodeNameFieldName]
            : "[no name]";

        var culture = context.GetCulture()?.ToLowerInvariant();
        if (culture.IsNullOrWhiteSpace() == false)
        {
            target.Name = source.Values.ContainsKey($"nodeName_{culture}")
                ? source.Values[$"nodeName_{culture}"].FirstOrDefault().ToString()
                : target.Name;
        }

        if (source.Values.TryGetValue(UmbracoSearchFieldNames.UmbracoFileFieldName, out var umbracoFile) &&
            umbracoFile.IsNullOrWhiteSpace() == false)
        {
            if (umbracoFile != null)
            {
                target.Name = $"{target.Name} ({umbracoFile})";
            }
        }

        if (source.Values.ContainsKey(UmbracoSearchFieldNames.NodeKeyFieldName))
        {
            if (Guid.TryParse(source.Values[UmbracoSearchFieldNames.NodeKeyFieldName], out Guid key))
            {
                target.Key = key;

                // need to set the UDI
                if (source.Values.ContainsKey(ExamineFieldNames.CategoryFieldName))
                {
                    switch (source.Values[ExamineFieldNames.CategoryFieldName].FirstOrDefault())
                    {
                        case IndexTypes.Member:
                            target.Udi = new GuidUdi(Constants.UdiEntityType.Member, target.Key);
                            break;
                        case IndexTypes.Content:
                            target.Udi = new GuidUdi(Constants.UdiEntityType.Document, target.Key);
                            break;
                        case IndexTypes.Media:
                            target.Udi = new GuidUdi(Constants.UdiEntityType.Media, target.Key);
                            break;
                    }
                }
            }
        }

        if (source.Values.ContainsKey("parentID"))
        {
            if (int.TryParse(source.Values["parentID"].FirstOrDefault(), NumberStyles.Integer, CultureInfo.InvariantCulture,
                    out var parentId))
            {
                target.ParentId = parentId;
            }
            else
            {
                target.ParentId = -1;
            }
        }

        target.Path = source.Values.ContainsKey(UmbracoSearchFieldNames.IndexPathFieldName)
            ? source.Values[UmbracoSearchFieldNames.IndexPathFieldName]
            : string.Empty;

        if (source.Values.ContainsKey(ExamineFieldNames.ItemTypeFieldName))
        {
            target.AdditionalData.Add("contentType", source.Values[ExamineFieldNames.ItemTypeFieldName]);
        }
    }

}
