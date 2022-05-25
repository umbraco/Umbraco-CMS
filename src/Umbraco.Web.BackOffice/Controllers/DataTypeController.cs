using System.Data;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Filters;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

/// <summary>
///     The API controller used for editing data types
/// </summary>
/// <remarks>
///     The security for this controller is defined to allow full CRUD access to data types if the user has access to
///     either:
///     Content Types, Member Types or Media Types ... and of course to Data Types
/// </remarks>
[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentsOrDocumentTypes)]
[ParameterSwapControllerActionSelector(nameof(GetById), "id", typeof(int), typeof(Guid), typeof(Udi))]
public class DataTypeController : BackOfficeNotificationsController
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly ContentSettings _contentSettings;
    private readonly IContentTypeService _contentTypeService;
    private readonly IDataTypeService _dataTypeService;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IMemberTypeService _memberTypeService;
    private readonly PropertyEditorCollection _propertyEditorCollection;
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly IConfigurationEditorJsonSerializer _serializer;
    private readonly IUmbracoMapper _umbracoMapper;

    public DataTypeController(
        PropertyEditorCollection propertyEditors,
        IDataTypeService dataTypeService,
        IOptionsSnapshot<ContentSettings> contentSettings,
        IUmbracoMapper umbracoMapper,
        PropertyEditorCollection propertyEditorCollection,
        IContentTypeService contentTypeService,
        IMediaTypeService mediaTypeService,
        IMemberTypeService memberTypeService,
        ILocalizedTextService localizedTextService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IConfigurationEditorJsonSerializer serializer)
    {
        _propertyEditors = propertyEditors ?? throw new ArgumentNullException(nameof(propertyEditors));
        _dataTypeService = dataTypeService ?? throw new ArgumentNullException(nameof(dataTypeService));
        _contentSettings = contentSettings.Value ?? throw new ArgumentNullException(nameof(contentSettings));
        _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
        _propertyEditorCollection = propertyEditorCollection ??
                                    throw new ArgumentNullException(nameof(propertyEditorCollection));
        _contentTypeService = contentTypeService ?? throw new ArgumentNullException(nameof(contentTypeService));
        _mediaTypeService = mediaTypeService ?? throw new ArgumentNullException(nameof(mediaTypeService));
        _memberTypeService = memberTypeService ?? throw new ArgumentNullException(nameof(memberTypeService));
        _localizedTextService = localizedTextService ?? throw new ArgumentNullException(nameof(localizedTextService));
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor ??
                                      throw new ArgumentNullException(nameof(backOfficeSecurityAccessor));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    /// <summary>
    ///     Gets data type by name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public DataTypeDisplay? GetByName(string name)
    {
        IDataType? dataType = _dataTypeService.GetDataType(name);
        return dataType == null ? null : _umbracoMapper.Map<IDataType, DataTypeDisplay>(dataType);
    }

    /// <summary>
    ///     Gets the datatype json for the datatype id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ActionResult<DataTypeDisplay?> GetById(int id)
    {
        IDataType? dataType = _dataTypeService.GetDataType(id);
        if (dataType == null)
        {
            return NotFound();
        }

        return _umbracoMapper.Map<IDataType, DataTypeDisplay>(dataType);
    }

    /// <summary>
    ///     Gets the datatype json for the datatype guid
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ActionResult<DataTypeDisplay?> GetById(Guid id)
    {
        IDataType? dataType = _dataTypeService.GetDataType(id);
        if (dataType == null)
        {
            return NotFound();
        }

        return _umbracoMapper.Map<IDataType, DataTypeDisplay>(dataType);
    }

    /// <summary>
    ///     Gets the datatype json for the datatype udi
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ActionResult<DataTypeDisplay?> GetById(Udi id)
    {
        var guidUdi = id as GuidUdi;
        if (guidUdi == null)
        {
            return NotFound();
        }

        IDataType? dataType = _dataTypeService.GetDataType(guidUdi.Guid);
        if (dataType == null)
        {
            return NotFound();
        }

        return _umbracoMapper.Map<IDataType, DataTypeDisplay>(dataType);
    }

    /// <summary>
    ///     Deletes a data type with a given ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete]
    [HttpPost]
    public IActionResult DeleteById(int id)
    {
        IDataType? foundType = _dataTypeService.GetDataType(id);
        if (foundType == null)
        {
            return NotFound();
        }

        IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
        _dataTypeService.Delete(foundType, currentUser?.Id ?? -1);

        return Ok();
    }

    public DataTypeDisplay? GetEmpty(int parentId)
    {
        // cannot create an "empty" data type, so use something by default.
        IDataEditor? editor = _propertyEditors[Constants.PropertyEditors.Aliases.Label];
        var dt = new DataType(editor, _serializer, parentId);
        return _umbracoMapper.Map<IDataType, DataTypeDisplay>(dt);
    }

    /// <summary>
    ///     Returns a custom listview, based on a content type alias, if found
    /// </summary>
    /// <param name="contentTypeAlias"></param>
    /// <returns>a DataTypeDisplay</returns>
    public ActionResult<DataTypeDisplay?> GetCustomListView(string contentTypeAlias)
    {
        IDataType? dt = _dataTypeService.GetDataType(Constants.Conventions.DataTypes.ListViewPrefix + contentTypeAlias);
        if (dt == null)
        {
            return NotFound();
        }

        return _umbracoMapper.Map<IDataType, DataTypeDisplay>(dt);
    }

    /// <summary>
    ///     Creates a custom list view - give a document type alias
    /// </summary>
    /// <param name="contentTypeAlias"></param>
    /// <returns></returns>
    public DataTypeDisplay? PostCreateCustomListView(string contentTypeAlias)
    {
        IDataType? dt = _dataTypeService.GetDataType(Constants.Conventions.DataTypes.ListViewPrefix + contentTypeAlias);

        //if it doesn't exist yet, we will create it.
        if (dt == null)
        {
            IDataEditor? editor = _propertyEditors[Constants.PropertyEditors.Aliases.ListView];
            dt = new DataType(editor, _serializer)
            {
                Name = Constants.Conventions.DataTypes.ListViewPrefix + contentTypeAlias
            };
            _dataTypeService.Save(dt);
        }

        return _umbracoMapper.Map<IDataType, DataTypeDisplay>(dt);
    }

    /// <summary>
    ///     Returns the pre-values for the specified property editor
    /// </summary>
    /// <param name="editorAlias"></param>
    /// <param name="dataTypeId">The data type id for the pre-values, -1 if it is a new data type</param>
    /// <returns></returns>
    public ActionResult<IEnumerable<DataTypeConfigurationFieldDisplay>> GetPreValues(string editorAlias, int dataTypeId = -1)
    {
        IDataEditor? propEd = _propertyEditors[editorAlias];
        if (propEd == null)
        {
            throw new InvalidOperationException("Could not find property editor with alias " + editorAlias);
        }

        if (dataTypeId == -1)
        {
            //this is a new data type, so just return the field editors with default values
            return new ActionResult<IEnumerable<DataTypeConfigurationFieldDisplay>>(
                _umbracoMapper.Map<IDataEditor, IEnumerable<DataTypeConfigurationFieldDisplay>>(propEd) ??
                Enumerable.Empty<DataTypeConfigurationFieldDisplay>());
        }

        //we have a data type associated
        IDataType? dataType = _dataTypeService.GetDataType(dataTypeId);
        if (dataType == null)
        {
            return NotFound();
        }

        //now, lets check if the data type has the current editor selected, if that is true
        //we will need to wire up it's saved values. Otherwise it's an existing data type
        //that is changing it's underlying property editor, in which case there's no values.
        if (dataType.EditorAlias == editorAlias)
        {
            //this is the currently assigned pre-value editor, return with values.
            return new ActionResult<IEnumerable<DataTypeConfigurationFieldDisplay>>(
                _umbracoMapper.Map<IDataType, IEnumerable<DataTypeConfigurationFieldDisplay>>(dataType) ??
                Enumerable.Empty<DataTypeConfigurationFieldDisplay>());
        }

        //these are new pre-values, so just return the field editors with default values
        return new ActionResult<IEnumerable<DataTypeConfigurationFieldDisplay>>(
            _umbracoMapper.Map<IDataEditor, IEnumerable<DataTypeConfigurationFieldDisplay>>(propEd) ??
            Enumerable.Empty<DataTypeConfigurationFieldDisplay>());
    }

    /// <summary>
    ///     Deletes a data type container with a given ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete]
    [HttpPost]
    public IActionResult DeleteContainer(int id)
    {
        IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
        _dataTypeService.DeleteContainer(id, currentUser?.Id ?? -1);

        return Ok();
    }

    public IActionResult PostCreateContainer(int parentId, string name)
    {
        IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
        Attempt<OperationResult<OperationResultType, EntityContainer>?> result =
            _dataTypeService.CreateContainer(parentId, Guid.NewGuid(), name, currentUser?.Id ?? -1);

        if (result.Success)
        {
            return Ok(result.Result); //return the id
        }

        return ValidationProblem(result.Exception?.Message);
    }

    /// <summary>
    ///     Saves the data type
    /// </summary>
    /// <param name="dataType"></param>
    /// <returns></returns>
    [DataTypeValidate]
    public ActionResult<DataTypeDisplay?> PostSave(DataTypeSave dataType)
    {
        //If we've made it here, then everything has been wired up and validated by the attribute

        // TODO: Check if the property editor has changed, if it has ensure we don't pass the
        // existing values to the new property editor!

        // get the current configuration,
        // get the new configuration as a dictionary (this is how we get it from model)
        // and map to an actual configuration object
        var currentConfiguration = dataType.PersistedDataType?.Configuration;
        var configurationDictionary = dataType.ConfigurationFields?.ToDictionary(x => x.Key, x => x.Value);
        var configuration = dataType.PropertyEditor?.GetConfigurationEditor()
            .FromConfigurationEditor(configurationDictionary, currentConfiguration);

        if (dataType.PersistedDataType is not null)
        {
            dataType.PersistedDataType.Configuration = configuration;
        }

        IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
        // save the data type
        try
        {
            if (dataType.PersistedDataType is not null)
            {
                _dataTypeService.Save(dataType.PersistedDataType, currentUser?.Id ?? -1);
            }
        }
        catch (DuplicateNameException ex)
        {
            ModelState.AddModelError("Name", ex.Message);
            return ValidationProblem(ModelState);
        }

        // map back to display model, and return
        DataTypeDisplay? display = _umbracoMapper.Map<IDataType, DataTypeDisplay>(dataType.PersistedDataType);
        display?.AddSuccessNotification(_localizedTextService.Localize("speechBubbles", "dataTypeSaved"), string.Empty);
        return display;
    }

    /// <summary>
    ///     Move the media type
    /// </summary>
    /// <param name="move"></param>
    /// <returns></returns>
    public IActionResult PostMove(MoveOrCopy move)
    {
        IDataType? toMove = _dataTypeService.GetDataType(move.Id);
        if (toMove == null)
        {
            return NotFound();
        }

        Attempt<OperationResult<MoveOperationStatusType>?> result = _dataTypeService.Move(toMove, move.ParentId);
        if (result.Success)
        {
            return Content(toMove.Path, MediaTypeNames.Text.Plain, Encoding.UTF8);
        }

        switch (result.Result?.Result)
        {
            case MoveOperationStatusType.FailedParentNotFound:
                return NotFound();
            case MoveOperationStatusType.FailedCancelledByEvent:
                return ValidationProblem();
            case MoveOperationStatusType.FailedNotAllowedByPath:
                var notificationModel = new SimpleNotificationModel();
                notificationModel.AddErrorNotification(_localizedTextService.Localize("moveOrCopy", "notAllowedByPath"), string.Empty);
                return ValidationProblem(notificationModel);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public IActionResult PostRenameContainer(int id, string name)
    {
        IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
        Attempt<OperationResult<OperationResultType, EntityContainer>?> result =
            _dataTypeService.RenameContainer(id, name, currentUser?.Id ?? -1);

        if (result.Success)
        {
            return Ok(result.Result);
        }

        return ValidationProblem(result.Exception?.Message);
    }

    /// <summary>
    ///     Returns the references (usages) for the data type
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public DataTypeReferences GetReferences(int id)
    {
        var result = new DataTypeReferences();
        IReadOnlyDictionary<Udi, IEnumerable<string>> usages = _dataTypeService.GetReferences(id);

        foreach (IGrouping<string, KeyValuePair<Udi, IEnumerable<string>>> groupOfEntityType in usages.GroupBy(x =>
                     x.Key.EntityType))
        {
            //get all the GUIDs for the content types to find
            var guidsAndPropertyAliases = groupOfEntityType.ToDictionary(i => ((GuidUdi)i.Key).Guid, i => i.Value);

            if (groupOfEntityType.Key == UmbracoObjectTypes.DocumentType.GetUdiType())
            {
                result.DocumentTypes = GetContentTypeUsages(_contentTypeService.GetAll(guidsAndPropertyAliases.Keys), guidsAndPropertyAliases);
            }
            else if (groupOfEntityType.Key == UmbracoObjectTypes.MediaType.GetUdiType())
            {
                result.MediaTypes = GetContentTypeUsages(_mediaTypeService.GetAll(guidsAndPropertyAliases.Keys), guidsAndPropertyAliases);
            }
            else if (groupOfEntityType.Key == UmbracoObjectTypes.MemberType.GetUdiType())
            {
                result.MemberTypes = GetContentTypeUsages(_memberTypeService.GetAll(guidsAndPropertyAliases.Keys), guidsAndPropertyAliases);
            }
        }

        return result;
    }

    /// <summary>
    ///     Maps the found content types and usages to the resulting model
    /// </summary>
    /// <param name="cts"></param>
    /// <param name="usages"></param>
    /// <returns></returns>
    private IEnumerable<DataTypeReferences.ContentTypeReferences> GetContentTypeUsages(
        IEnumerable<IContentTypeBase> cts,
        IReadOnlyDictionary<Guid, IEnumerable<string>> usages) =>
        cts.Select(x => new DataTypeReferences.ContentTypeReferences
        {
            Id = x.Id,
            Key = x.Key,
            Alias = x.Alias,
            Icon = x.Icon,
            Name = x.Name,
            Udi = new GuidUdi(UmbracoObjectTypes.DocumentType.GetUdiType(), x.Key),
            //only select matching properties
            Properties = x.PropertyTypes.Where(p => usages[x.Key].InvariantContains(p.Alias))
                .Select(p => new DataTypeReferences.ContentTypeReferences.PropertyTypeReferences
                {
                    Alias = p.Alias,
                    Name = p.Name
                })
        });

    #region ReadOnly actions to return basic data - allow access for: content ,media, members, settings, developer

    /// <summary>
    ///     Gets the content json for all data types
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    ///     Permission is granted to this method if the user has access to any of these sections: Content, media, settings,
    ///     developer, members
    /// </remarks>
    [Authorize(Policy = AuthorizationPolicies.SectionAccessForDataTypeReading)]
    public IEnumerable<DataTypeBasic>? GetAll() =>
        _dataTypeService
            .GetAll()
            .Select(_umbracoMapper.Map<IDataType, DataTypeBasic>).WhereNotNull()
            .Where(x => x.IsSystemDataType == false);

    /// <summary>
    ///     Returns all data types grouped by their property editor group
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    ///     Permission is granted to this method if the user has access to any of these sections: Content, media, settings,
    ///     developer, members
    /// </remarks>
    [Authorize(Policy = AuthorizationPolicies.SectionAccessForDataTypeReading)]
    public IDictionary<string, IEnumerable<DataTypeBasic>>? GetGroupedDataTypes()
    {
        DataTypeBasic?[]? dataTypes = _dataTypeService
            .GetAll()
            .Select(_umbracoMapper.Map<IDataType, DataTypeBasic>)
            .ToArray();

        IDataEditor[] propertyEditors = _propertyEditorCollection.ToArray();

        if (dataTypes is not null)
        {
            foreach (DataTypeBasic? dataType in dataTypes)
            {
                IDataEditor? propertyEditor = propertyEditors.SingleOrDefault(x => x.Alias == dataType?.Alias);
                if (propertyEditor != null && dataType is not null)
                {
                    dataType.HasPrevalues = propertyEditor.GetConfigurationEditor().Fields.Any();
                }
            }
        }

        var grouped = dataTypes?.WhereNotNull()
            .GroupBy(x => x.Group.IsNullOrWhiteSpace() ? string.Empty : x.Group!.ToLower())
            .ToDictionary(group => group.Key, group => group.OrderBy(d => d.Name).AsEnumerable());

        return grouped;
    }

    /// <summary>
    ///     Returns all property editors grouped
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    ///     Permission is granted to this method if the user has access to any of these sections: Content, media, settings,
    ///     developer, members
    /// </remarks>
    [Authorize(Policy = AuthorizationPolicies.SectionAccessForDataTypeReading)]
    public IDictionary<string, IEnumerable<DataTypeBasic>> GetGroupedPropertyEditors()
    {
        var datatypes = new List<DataTypeBasic>();
        var showDeprecatedPropertyEditors = _contentSettings.ShowDeprecatedPropertyEditors;

        IEnumerable<IDataEditor> propertyEditors = _propertyEditorCollection
            .Where(x => x.IsDeprecated == false || showDeprecatedPropertyEditors);
        foreach (IDataEditor propertyEditor in propertyEditors)
        {
            var hasPrevalues = propertyEditor.GetConfigurationEditor().Fields.Any();
            DataTypeBasic? basic = _umbracoMapper.Map<DataTypeBasic>(propertyEditor);
            if (basic is not null)
            {
                basic.HasPrevalues = hasPrevalues;
                datatypes.Add(basic);
            }
        }

        var grouped = datatypes
            .GroupBy(x => x.Group.IsNullOrWhiteSpace()
                ? string.Empty
                : x.Group!.ToLower())
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(d => d.Name)
                    .AsEnumerable());

        return grouped;
    }


    /// <summary>
    ///     Gets all property editors defined
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    ///     Permission is granted to this method if the user has access to any of these sections: Content, media, settings,
    ///     developer, members
    /// </remarks>
    [Authorize(Policy = AuthorizationPolicies.SectionAccessForDataTypeReading)]
    public IEnumerable<PropertyEditorBasic> GetAllPropertyEditors() =>
        _propertyEditorCollection
            .OrderBy(x => x.Name)
            .Select(_umbracoMapper.Map<PropertyEditorBasic>).WhereNotNull();

    #endregion
}
