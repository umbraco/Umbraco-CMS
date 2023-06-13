using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Differencing;
using Polly;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Trees;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Web.UI;

public class VersioningComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<MenuRenderingNotification, MenuRenderingNotificationHandler>();
    }
}

public class MenuRenderingNotificationHandler : INotificationHandler<MenuRenderingNotification>
{
    public void Handle(MenuRenderingNotification notification)
    {
        if (!notification.TreeAlias.Equals(Core.Constants.Trees.Content))
        {
            return;
        }

        MenuItem m = new("createVersion", "Create version");
        m.AdditionalData.Add("actionView", "/App_Plugins/Versioning/CreateVersion.html");
        m.Icon = "umb-content";
        notification.Menu.Items.Insert(0, m);
    }
}

[PluginController("Versioning")]
public class VersioningController : UmbracoAuthorizedApiController
{
    private readonly Guid _nodeObjectTypeId = Core.Constants.ObjectTypes.Document;
    private readonly IScopeProvider _scopeProvider;
    private readonly IContentService _contentService;
    private readonly IDocumentRepository _documentRepository;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUmbracoMapper _umbracoMapper;

    public VersioningController(
        IContentService contentService,
        IScopeProvider scopeProvider,
        IDocumentRepository documentRepository,
        IUmbracoMapper umbracoMapper,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _contentService = contentService;
        _scopeProvider = scopeProvider;
        _documentRepository = documentRepository;
        _umbracoMapper = umbracoMapper;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    public IActionResult CreateAlternateVersion(int nodeId, string? name)
    {
        IContent? node = _contentService.GetById(nodeId);

        if (node is null)
        {
            return BadRequest();
        }

        int? versionId = CreateVersion(node);

        return Ok(new { versionId });
    }

    [HttpGet]
    public IActionResult GetAlternateVersions(int nodeId)
    {
        using IScope scope = _scopeProvider.CreateScope();
        List<ContentVersionDto> versions = scope.Database.Fetch<ContentVersionDto>($"SELECT * FROM umbracoContentVersion WHERE nodeId = {nodeId} AND alternate = 1");

        _ = scope.Complete();

        var result =versions.Select(x => new
        {
            text = x.Text,
            versionId = x.Id,
            versionDate = x.VersionDate,
        });

        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicies.ContentPermissionBrowseById)]
    public ActionResult<ContentItemDisplayWithSchedule?>? GetByVersionId(int versionId)
    {
        IContent? foundContent = _contentService.GetVersion(versionId);
        if (foundContent == null)
        {
            return null;
        }

        return MapToDisplayWithSchedule(foundContent);
    }

    private int? CreateVersion(IContent entity)
    {
        DocumentDto? dto = GetDocumentDto(entity);

        if (dto is null)
        {
            return null;
        }

        int contentVersionDtoId = PersistVersions(entity, dto);
        UpdatePropertyValues(entity, contentVersionDtoId);

        return contentVersionDtoId;
    }

    private int PersistVersions(IContent entity, DocumentDto dto)
    {
        ContentVersionDto contentVersionDto = dto.DocumentVersionDto.ContentVersionDto;
        DocumentVersionDto documentVersionDto = dto.DocumentVersionDto;

        contentVersionDto.Id = 0;
        contentVersionDto.Current = false;
        contentVersionDto.Alternate = true;
        contentVersionDto.Text = entity.Name;
        contentVersionDto.PreventCleanup = true;
        contentVersionDto.VersionDate = DateTime.Now;

        using IScope scope = _scopeProvider.CreateScope();
        _ = scope.Database.Insert(contentVersionDto);

        entity.VersionId = documentVersionDto.Id = contentVersionDto.Id;
        _ = scope.Database.Insert(documentVersionDto);

        _ = scope.Complete();

        return contentVersionDto.Id;
    }

    private void UpdatePropertyValues(IContent entity, int id)
    {
        bool edited = false;
        HashSet<string>? editedCultures = new();

        // stuff property values with correct version id
        MethodInfo? insertPropertyValues = typeof(DocumentRepository).BaseType?
            .GetMethod(
                "InsertPropertyValues",
                bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

        using IScope scope = _scopeProvider.CreateScope();
        _ = insertPropertyValues?.Invoke(_documentRepository, new object[] { entity, id, edited, editedCultures });
        _ = scope.Complete();
    }

    private DocumentDto? GetDocumentDto(IContent entity)
    {
        (object contentBaseFactoryInstance, Type contentBaseFactoryType) = ReflectionTools.InstanceActivator(
            typeof(DocumentRepository).Assembly,
            "Umbraco.Cms.Infrastructure.Persistence.Factories.ContentBaseFactory");

        MethodInfo? buildDto = contentBaseFactoryType.GetMethod("BuildDto", new Type[] { typeof(IContent), typeof(Guid) });

        if (buildDto?.Invoke(contentBaseFactoryInstance, new object[] { entity, _nodeObjectTypeId }) is not DocumentDto dto)
        {
            return null;
        }

        return dto;
    }

    private ContentItemDisplayWithSchedule? MapToDisplayWithSchedule(IContent? content)
    {
        if (content is null)
        {
            return null;
        }

        ContentItemDisplayWithSchedule? display = _umbracoMapper.Map<ContentItemDisplayWithSchedule>(content, context =>
        {
            context.Items["CurrentUser"] = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
            context.Items["Schedule"] = _contentService.GetContentScheduleByContentId(content.Id);
        });

        if (display is not null)
        {
            display.AllowPreview = display.AllowPreview && content?.Trashed == false &&
                                   content.ContentType.IsElement == false;
        }

        return display;
    }
}

public static class ReflectionTools
{
    public static (object Instance, Type TypeFromAssembly) InstanceActivator(Assembly assembly, string type, object?[]? constructorParameters = null)
    {
        Type? typeFromAssembly = assembly.GetType(type);

        if (typeFromAssembly is null)
        {
            throw new NullReferenceException(nameof(typeFromAssembly));
        }

        object? instance = Activator.CreateInstance(
            typeFromAssembly,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            constructorParameters,
            null);

        if (instance is null)
        {
            throw new NullReferenceException(nameof(instance));
        }

        return (instance, typeFromAssembly);
    }
}

