using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

/// <summary>
///     The API controller for editing relation types.
/// </summary>
[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
[Authorize(Policy = AuthorizationPolicies.TreeAccessRelationTypes)]
[ParameterSwapControllerActionSelector(nameof(GetById), "id", typeof(int), typeof(Guid), typeof(Udi))]
public class RelationTypeController : BackOfficeNotificationsController
{
    private readonly ILogger<RelationTypeController> _logger;
    private readonly IRelationService _relationService;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IUmbracoMapper _umbracoMapper;

    public RelationTypeController(
        ILogger<RelationTypeController> logger,
        IUmbracoMapper umbracoMapper,
        IRelationService relationService,
        IShortStringHelper shortStringHelper)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
        _relationService = relationService ?? throw new ArgumentNullException(nameof(relationService));
        _shortStringHelper = shortStringHelper ?? throw new ArgumentNullException(nameof(shortStringHelper));
    }

    /// <summary>
    ///     Gets a relation type by id
    /// </summary>
    /// <param name="id">The relation type ID.</param>
    /// <returns>Returns the <see cref="RelationTypeDisplay" />.</returns>
    public ActionResult<RelationTypeDisplay?> GetById(int id)
    {
        IRelationType? relationType = _relationService.GetRelationTypeById(id);

        if (relationType == null)
        {
            return NotFound();
        }

        RelationTypeDisplay? display = _umbracoMapper.Map<IRelationType, RelationTypeDisplay>(relationType);

        return display;
    }

    /// <summary>
    ///     Gets a relation type by guid
    /// </summary>
    /// <param name="id">The relation type ID.</param>
    /// <returns>Returns the <see cref="RelationTypeDisplay" />.</returns>
    public ActionResult<RelationTypeDisplay?> GetById(Guid id)
    {
        IRelationType? relationType = _relationService.GetRelationTypeById(id);
        if (relationType == null)
        {
            return NotFound();
        }

        return _umbracoMapper.Map<IRelationType, RelationTypeDisplay>(relationType);
    }

    /// <summary>
    ///     Gets a relation type by udi
    /// </summary>
    /// <param name="id">The relation type ID.</param>
    /// <returns>Returns the <see cref="RelationTypeDisplay" />.</returns>
    public ActionResult<RelationTypeDisplay?> GetById(Udi id)
    {
        var guidUdi = id as GuidUdi;
        if (guidUdi == null)
        {
            return NotFound();
        }

        IRelationType? relationType = _relationService.GetRelationTypeById(guidUdi.Guid);
        if (relationType == null)
        {
            return NotFound();
        }

        return _umbracoMapper.Map<IRelationType, RelationTypeDisplay>(relationType);
    }

    public PagedResult<RelationDisplay?> GetPagedResults(int id, int pageNumber = 1, int pageSize = 100)
    {
        if (pageNumber <= 0 || pageSize <= 0)
        {
            throw new NotSupportedException("Both pageNumber and pageSize must be greater than zero");
        }

        // Ordering do we need to pass through?
        IEnumerable<IRelation> relations =
            _relationService.GetPagedByRelationTypeId(id, pageNumber - 1, pageSize, out var totalRecords);

        return new PagedResult<RelationDisplay?>(totalRecords, pageNumber, pageSize)
        {
            Items = relations.Select(x => _umbracoMapper.Map<RelationDisplay>(x))
        };
    }

    /// <summary>
    ///     Gets a list of object types which can be associated via relations.
    /// </summary>
    /// <returns>A list of available object types.</returns>
    public List<ObjectType> GetRelationObjectTypes()
    {
        var objectTypes = new List<ObjectType>
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

        return objectTypes;
    }

    /// <summary>
    ///     Creates a new relation type.
    /// </summary>
    /// <param name="relationType">The relation type to create.</param>
    /// <returns>A <see cref="HttpResponseMessage" /> containing the persisted relation type's ID.</returns>
    public ActionResult<int> PostCreate(RelationTypeSave relationType)
    {
        var relationTypePersisted = new RelationType(
            relationType.Name,
            relationType.Name?.ToSafeAlias(_shortStringHelper, true),
            relationType.IsBidirectional,
            relationType.ParentObjectType,
            relationType.ChildObjectType,
            relationType.IsDependency);

        try
        {
            _relationService.Save(relationTypePersisted);

            return relationTypePersisted.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating relation type with {Name}", relationType.Name);
            return ValidationProblem("Error creating relation type.");
        }
    }

    /// <summary>
    ///     Updates an existing relation type.
    /// </summary>
    /// <param name="relationType">The relation type to update.</param>
    /// <returns>A display object containing the updated relation type.</returns>
    public ActionResult<RelationTypeDisplay?> PostSave(RelationTypeSave relationType)
    {
        IRelationType? relationTypePersisted = _relationService.GetRelationTypeById(relationType.Key);

        if (relationTypePersisted == null)
        {
            return ValidationProblem("Relation type does not exist");
        }

        _umbracoMapper.Map(relationType, relationTypePersisted);

        try
        {
            _relationService.Save(relationTypePersisted);
            RelationTypeDisplay? display = _umbracoMapper.Map<RelationTypeDisplay>(relationTypePersisted);
            display?.AddSuccessNotification("Relation type saved", "");

            return display;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving relation type with {Id}", relationType.Id);
            return ValidationProblem("Something went wrong when saving the relation type");
        }
    }

    /// <summary>
    ///     Deletes a relation type with a given ID.
    /// </summary>
    /// <param name="id">The ID of the relation type to delete.</param>
    /// <returns>A <see cref="HttpResponseMessage" />.</returns>
    [HttpPost]
    [HttpDelete]
    public IActionResult DeleteById(int id)
    {
        IRelationType? relationType = _relationService.GetRelationTypeById(id);

        if (relationType == null)
        {
            return NotFound();
        }

        _relationService.Delete(relationType);

        return Ok();
    }
}
