using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Services;

/// <summary>
/// Batch-optimized implementation of <see cref="ISearchResultAncestorService"/>.
/// Collects all unique ancestor IDs across all entities and resolves them efficiently.
/// For entity types whose ancestors support <see cref="IEntityService.GetAll(UmbracoObjectTypes, int[])"/>
/// (Document, Media, Template), a single batch call is used. For container-based ancestors
/// (DataType, DocumentType, MediaType folders), individual <see cref="IEntityService.Get(int)"/> calls
/// are used because <c>GetAll</c> does not support container object types.
/// For document ancestors, variant names are resolved using the default language culture.
/// </summary>
internal sealed class SearchResultAncestorService : ISearchResultAncestorService
{
    private readonly IEntityService _entityService;
    private readonly ILanguageService _languageService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchResultAncestorService"/> class.
    /// </summary>
    /// <param name="entityService">The entity service.</param>
    /// <param name="languageService">The language service.</param>
    public SearchResultAncestorService(IEntityService entityService, ILanguageService languageService)
    {
        _entityService = entityService;
        _languageService = languageService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, IReadOnlyList<SearchResultAncestorModel>>> ResolveAsync(
        IEnumerable<ITreeEntity> entities,
        UmbracoObjectTypes entityObjectType)
    {
        ITreeEntity[] entityArray = entities as ITreeEntity[] ?? entities.ToArray();
        if (entityArray.Length == 0)
        {
            return new Dictionary<Guid, IReadOnlyList<SearchResultAncestorModel>>();
        }

        UmbracoObjectTypes? ancestorObjectType = GetAncestorObjectType(entityObjectType);
        if (ancestorObjectType is null)
        {
            // Flat entity types (Member, MemberType) have no ancestors to resolve.
            return entityArray.ToDictionary(
                e => e.Key,
                _ => (IReadOnlyList<SearchResultAncestorModel>)[]);
        }

        // Collect all unique ancestor integer IDs across all entities.
        Dictionary<Guid, int[]> ancestorIdsByEntityKey = new(entityArray.Length);
        HashSet<int> allAncestorIds = [];

        foreach (ITreeEntity entity in entityArray)
        {
            var ancestorIds = entity.AncestorIds();
            ancestorIdsByEntityKey[entity.Key] = ancestorIds;
            foreach (var id in ancestorIds)
            {
                allAncestorIds.Add(id);
            }
        }

        if (allAncestorIds.Count == 0)
        {
            return entityArray.ToDictionary(
                e => e.Key,
                _ => (IReadOnlyList<SearchResultAncestorModel>)Array.Empty<SearchResultAncestorModel>());
        }

        // Fetch all ancestor entities.
        // IEntityService.GetAll(UmbracoObjectTypes, int[]) does not support container types
        // (DataTypeContainer, DocumentTypeContainer, MediaTypeContainer), so for those we
        // fall back to individual Get(int) calls. Container ancestor sets are typically small
        // (shallow folder trees), so this is acceptable.
        Dictionary<int, IEntitySlim> ancestorLookup = IsContainerType(ancestorObjectType.Value)
            ? ResolveByIndividualGet(allAncestorIds)
            : _entityService.GetAll(ancestorObjectType.Value, allAncestorIds.ToArray()).ToDictionary(e => e.Id);

        // Resolve the default culture for variant document name resolution.
        string? defaultCulture = entityObjectType == UmbracoObjectTypes.Document
            ? await _languageService.GetDefaultIsoCodeAsync()
            : null;

        // Build the result: for each entity, walk ancestor IDs in path order and build the ancestor list.
        var result = new Dictionary<Guid, IReadOnlyList<SearchResultAncestorModel>>(entityArray.Length);
        foreach (ITreeEntity entity in entityArray)
        {
            var ancestorIds = ancestorIdsByEntityKey[entity.Key];
            var ancestors = new List<SearchResultAncestorModel>(ancestorIds.Length);

            foreach (var ancestorId in ancestorIds)
            {
                if (ancestorLookup.TryGetValue(ancestorId, out IEntitySlim? ancestor))
                {
                    ancestors.Add(new SearchResultAncestorModel
                    {
                        Id = ancestor.Key,
                        Name = ResolveAncestorName(ancestor, defaultCulture),
                        EntityType = ObjectTypes.GetUdiType(ancestor.NodeObjectType),
                    });
                }
            }

            result[entity.Key] = ancestors;
        }

        return result;
    }

    private static bool IsContainerType(UmbracoObjectTypes objectType)
        => objectType is UmbracoObjectTypes.DataTypeContainer
            or UmbracoObjectTypes.DocumentTypeContainer
            or UmbracoObjectTypes.MediaTypeContainer;

    private Dictionary<int, IEntitySlim> ResolveByIndividualGet(HashSet<int> ids)
    {
        var lookup = new Dictionary<int, IEntitySlim>(ids.Count);
        foreach (var id in ids)
        {
            IEntitySlim? entity = _entityService.Get(id);
            if (entity is not null)
            {
                lookup[entity.Id] = entity;
            }
        }

        return lookup;
    }

    private static UmbracoObjectTypes? GetAncestorObjectType(UmbracoObjectTypes entityObjectType)
        => entityObjectType switch
        {
            UmbracoObjectTypes.DataType => UmbracoObjectTypes.DataTypeContainer,
            UmbracoObjectTypes.DocumentType => UmbracoObjectTypes.DocumentTypeContainer,
            UmbracoObjectTypes.MediaType => UmbracoObjectTypes.MediaTypeContainer,
            UmbracoObjectTypes.Document => UmbracoObjectTypes.Document,
            UmbracoObjectTypes.Media => UmbracoObjectTypes.Media,
            UmbracoObjectTypes.Template => UmbracoObjectTypes.Template,
            _ => null,
        };

    private static string ResolveAncestorName(IEntitySlim ancestor, string? defaultCulture)
    {
        if (defaultCulture is not null && ancestor is IDocumentEntitySlim documentEntity && documentEntity.CultureNames.Count > 0)
        {
            return documentEntity.CultureNames.TryGetValue(defaultCulture, out var cultureName)
                ? cultureName
                : ancestor.Name ?? string.Empty;
        }

        return ancestor.Name ?? string.Empty;
    }
}
