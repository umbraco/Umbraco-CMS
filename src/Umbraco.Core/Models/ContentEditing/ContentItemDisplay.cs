using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Core.Models.ContentEditing;

public class ContentItemDisplay : ContentItemDisplay<ContentVariantDisplay>
{
}

public class ContentItemDisplayWithSchedule : ContentItemDisplay<ContentVariantScheduleDisplay>
{
}

/// <summary>
///     A model representing a content item to be displayed in the back office
/// </summary>
[DataContract(Name = "content", Namespace = "")]
public class ContentItemDisplay<TVariant> :
    INotificationModel,
    IErrorModel // ListViewAwareContentItemDisplayBase<ContentPropertyDisplay, IContent>
    where TVariant : ContentVariantDisplay
{
    public ContentItemDisplay()
    {
        AllowPreview = true;
        Notifications = new List<BackOfficeNotification>();
        Errors = new Dictionary<string, object>();
        Variants = new List<TVariant>();
        ContentApps = new List<ContentApp>();
    }

    [DataMember(Name = "id", IsRequired = true)]
    [Required]
    public int Id { get; set; }

    [DataMember(Name = "udi")]
    [ReadOnly(true)]
    public Udi? Udi { get; set; }

    [DataMember(Name = "icon")]
    public string? Icon { get; set; }

    [DataMember(Name = "trashed")]
    [ReadOnly(true)]
    public bool Trashed { get; set; }

    /// <summary>
    ///     This is the unique Id stored in the database - but could also be the unique id for a custom membership provider
    /// </summary>
    [DataMember(Name = "key")]
    public Guid? Key { get; set; }

    [DataMember(Name = "parentId", IsRequired = true)]
    [Required]
    public int? ParentId { get; set; }

    /// <summary>
    ///     The path of the entity
    /// </summary>
    [DataMember(Name = "path")]
    public string? Path { get; set; }

    /// <summary>
    ///     A collection of content variants
    /// </summary>
    /// <remarks>
    ///     If a content item is invariant, this collection will only contain one item, else it will contain all culture
    ///     variants
    /// </remarks>
    [DataMember(Name = "variants")]
    public IEnumerable<TVariant> Variants { get; set; }

    [DataMember(Name = "owner")]
    public UserProfile? Owner { get; set; }

    [DataMember(Name = "updater")]
    public UserProfile? Updater { get; set; }

    /// <summary>
    ///     The name of the content type
    /// </summary>
    [DataMember(Name = "contentTypeName")]
    public string? ContentTypeName { get; set; }

    /// <summary>
    ///     Indicates if the content is configured as a list view container
    /// </summary>
    [DataMember(Name = "isContainer")]
    public bool IsContainer { get; set; }

    /// <summary>
    ///     Indicates if the content is configured as an element
    /// </summary>
    [DataMember(Name = "isElement")]
    public bool IsElement { get; set; }

    /// <summary>
    ///     Property indicating if this item is part of a list view parent
    /// </summary>
    [DataMember(Name = "isChildOfListView")]
    public bool IsChildOfListView { get; set; }

    /// <summary>
    ///     Property for the entity's individual tree node URL
    /// </summary>
    /// <remarks>
    ///     This is required if the item is a child of a list view since the tree won't actually be loaded,
    ///     so the app will need to go fetch the individual tree node in order to be able to load it's action list (menu)
    /// </remarks>
    [DataMember(Name = "treeNodeUrl")]
    public string? TreeNodeUrl { get; set; }

    [DataMember(Name = "contentTypeId")]
    public int? ContentTypeId { get; set; }

    [DataMember(Name = "contentTypeKey")]
    public Guid ContentTypeKey { get; set; }

    [DataMember(Name = "contentTypeAlias", IsRequired = true)]
    [Required(AllowEmptyStrings = false)]
    public string ContentTypeAlias { get; set; } = null!;

    [DataMember(Name = "sortOrder")]
    public int SortOrder { get; set; }

    /// <summary>
    ///     This is the last updated date for the entire content object regardless of variants
    /// </summary>
    /// <remarks>
    ///     Each variant has it's own update date assigned as well
    /// </remarks>
    [DataMember(Name = "updateDate")]
    public DateTime UpdateDate { get; set; }

    [DataMember(Name = "template")]
    public string? TemplateAlias { get; set; }

    [DataMember(Name = "templateId")]
    public int TemplateId { get; set; }

    [DataMember(Name = "allowedTemplates")]
    public IDictionary<string, string?>? AllowedTemplates { get; set; }

    [DataMember(Name = "documentType")]
    public ContentTypeBasic? DocumentType { get; set; }

    [DataMember(Name = "urls")]
    public UrlInfo[]? Urls { get; set; }

    /// <summary>
    ///     Determines whether previewing is allowed for this node
    /// </summary>
    /// <remarks>
    ///     By default this is true but by using events developers can toggle this off for certain documents if there is
    ///     nothing to preview
    /// </remarks>
    [DataMember(Name = "allowPreview")]
    public bool AllowPreview { get; set; }

    /// <summary>
    ///     The allowed 'actions' based on the user's permissions - Create, Update, Publish, Send to publish
    /// </summary>
    /// <remarks>
    ///     Each char represents a button which we can then map on the front-end to the correct actions
    /// </remarks>
    [DataMember(Name = "allowedActions")]
    public IEnumerable<string>? AllowedActions { get; set; }

    [DataMember(Name = "isBlueprint")]
    public bool IsBlueprint { get; set; }

    [DataMember(Name = "apps")]
    public IEnumerable<ContentApp> ContentApps { get; set; }

    /// <summary>
    ///     The real persisted content object - used during inbound model binding
    /// </summary>
    /// <remarks>
    ///     This is not used for outgoing model information.
    /// </remarks>
    [IgnoreDataMember]
    public IContent? PersistedContent { get; set; }

    /// <summary>
    ///     The DTO object used to gather all required content data including data type information etc... for use with
    ///     validation - used during inbound model binding
    /// </summary>
    /// <remarks>
    ///     We basically use this object to hydrate all required data from the database into one object so we can validate
    ///     everything we need
    ///     instead of having to look up all the data individually.
    ///     This is not used for outgoing model information.
    /// </remarks>
    [IgnoreDataMember]
    public ContentPropertyCollectionDto? ContentDto { get; set; }

    /// <summary>
    ///     A collection of extra data that is available for this specific entity/entity type
    /// </summary>
    [DataMember(Name = "metaData")]
    [ReadOnly(true)]
    public IDictionary<string, object>? AdditionalData { get; private set; }

    /// <summary>
    ///     This is used for validation of a content item.
    /// </summary>
    /// <remarks>
    ///     A content item can be invalid but still be saved. This occurs when there's property validation errors, we will
    ///     still save the item but it cannot be published. So we need a way of returning validation errors as well as the
    ///     updated model.
    ///     NOTE: The ProperCase is important because when we return ModeState normally it will always be proper case.
    /// </remarks>
    [DataMember(Name = "ModelState")]
    [ReadOnly(true)]
    public IDictionary<string, object> Errors { get; set; }

    /// <summary>
    ///     This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
    /// </summary>
    [DataMember(Name = "notifications")]
    [ReadOnly(true)]
    public List<BackOfficeNotification> Notifications { get; private set; }
}
