using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Factories.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents a repository for doing CRUD operations for <see cref="IMemberType" />
/// </summary>
/// <remarks>
///     The shared content-type-composition logic lives in <see cref="AsyncContentTypeRepositoryBase{TEntity}"/>;
///     this class only supplies the member-type specifics: the node object type, the built-in standard properties,
///     and the member-only property metadata (<see cref="MemberPropertyTypeDto"/>) persistence.
/// </remarks>
internal sealed class MemberTypeRepository : AsyncContentTypeRepositoryBase<IMemberType>, IMemberTypeRepository
{
    private readonly IShortStringHelper _shortStringHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberTypeRepository"/> class.
    /// </summary>
    public MemberTypeRepository(
        AppCaches cache,
        ILogger<MemberTypeRepository> logger,
        IContentTypeCommonRepository commonRepository,
        ILanguageRepository languageRepository,
        IShortStringHelper shortStringHelper,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        IIdKeyMap idKeyMap,
        ICacheSyncService cacheSyncService,
        IEFCoreScopeAccessor<UmbracoDbContext> efCoreScopeAccessor)
        : base(
            cache,
            logger,
            commonRepository,
            languageRepository,
            repositoryCacheVersionService,
            idKeyMap,
            cacheSyncService,
            efCoreScopeAccessor)
    {
        _shortStringHelper = shortStringHelper;
    }

    /// <inheritdoc />
    protected override Guid NodeObjectTypeId => Constants.ObjectTypes.MemberType;

    /// <inheritdoc />
    protected override bool SupportsPublishing => MemberType.SupportsPublishingConst;

    /// <inheritdoc />
    protected override async Task PersistNewItemAsync(IMemberType entity)
    {
        ValidateAlias(entity);

        entity.AddingEntity();

        // set a default icon if one is not specified
        if (entity.Icon.IsNullOrWhiteSpace())
        {
            entity.Icon = Constants.Icons.Member;
        }

        // By Convention we add 9 standard PropertyTypes to an Umbraco MemberType
        Dictionary<string, PropertyType> standardPropertyTypes =
            ConventionsHelper.GetStandardPropertyTypeStubs(_shortStringHelper);
        foreach (KeyValuePair<string, PropertyType> standardPropertyType in standardPropertyTypes)
        {
            entity.AddPropertyType(
                standardPropertyType.Value,
                Constants.Conventions.Member.StandardPropertiesGroupAlias,
                Constants.Conventions.Member.StandardPropertiesGroupName);
        }

        EnsureExplicitDataTypeForBuiltInProperties(entity);
        await PersistNewBaseContentTypeAsync(entity);

        await PersistMemberPropertyTypesAsync(entity);

        entity.ResetDirtyProperties();
    }

    /// <inheritdoc />
    protected override async Task PersistUpdatedItemAsync(IMemberType entity)
    {
        ValidateAlias(entity);

        // Updates Modified date
        entity.UpdatingEntity();

        // Look up parent to get and set the correct Path if ParentId has changed
        if (entity.IsPropertyDirty("ParentId"))
        {
            var parent = await ExecuteEfScopeAsync(db => db.Nodes
                .Where(x => x.NodeId == entity.ParentId)
                .Select(x => new { x.Path, x.Level })
                .FirstAsync());
            entity.Path = string.Concat(parent.Path, ",", entity.Id);
            entity.Level = parent.Level + 1;

            var maxSortOrder = await ExecuteEfScopeAsync(db => db.Nodes
                .Where(x => x.ParentId == entity.ParentId && x.NodeObjectType == NodeObjectTypeId)
                .Select(x => (int?)x.SortOrder)
                .MaxAsync()) ?? 0;
            entity.SortOrder = maxSortOrder + 1;
        }

        EnsureExplicitDataTypeForBuiltInProperties(entity);
        await PersistUpdatedBaseContentTypeAsync(entity);

        // remove and re-insert - handle the cmsMemberType table
        await ExecuteEfScopeAsync(db => db.MemberPropertyTypes.Where(x => x.NodeId == entity.Id).ExecuteDeleteAsync());
        await PersistMemberPropertyTypesAsync(entity);

        entity.ResetDirtyProperties();
    }

    /// <inheritdoc />
    protected override Task DeleteContentTypeSpecificDefinitionTablesAsync(UmbracoDbContext db, int id)
        => db.MemberPropertyTypes.Where(x => x.NodeId == id).ExecuteDeleteAsync();

    private Task PersistMemberPropertyTypesAsync(IMemberType entity)
        => ExecuteEfScopeAsync(async db =>
        {
            foreach (MemberPropertyTypeDto memberPropertyTypeDto in ContentTypeFactory.BuildMemberPropertyTypeDtos(entity))
            {
                db.MemberPropertyTypes.Add(memberPropertyTypeDto);
            }

            await db.SaveChangesAsync();
        });

    /// <summary>
    ///     Ensure that all the built-in membership provider properties have their correct data type
    ///     and property editors assigned. This occurs prior to saving so that the correct values are persisted.
    /// </summary>
    private void EnsureExplicitDataTypeForBuiltInProperties(IContentTypeBase memberType)
    {
        Dictionary<string, PropertyType> builtinProperties =
            ConventionsHelper.GetStandardPropertyTypeStubs(_shortStringHelper);
        foreach (IPropertyType propertyType in memberType.PropertyTypes)
        {
            // this reset's its current data type reference which will be re-assigned based on the property editor assigned on the next line
            if (builtinProperties.TryGetValue(propertyType.Alias, out PropertyType? propDefinition))
            {
                propertyType.DataTypeId = propDefinition.DataTypeId;
                propertyType.DataTypeKey = propDefinition.DataTypeKey;
            }
        }
    }
}
