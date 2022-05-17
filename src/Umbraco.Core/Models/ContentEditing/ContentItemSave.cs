using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Editors;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     A model representing a content item to be saved
/// </summary>
[DataContract(Name = "content", Namespace = "")]
public class ContentItemSave : IContentSave<IContent>
{
    public ContentItemSave()
    {
        UploadedFiles = new List<ContentPropertyFile>();
        Variants = new List<ContentVariantSave>();
    }

    [DataMember(Name = "id", IsRequired = true)]
    [Required]
    public int Id { get; set; }

    [DataMember(Name = "parentId", IsRequired = true)]
    [Required]
    public int ParentId { get; set; }

    [DataMember(Name = "variants", IsRequired = true)]
    public IEnumerable<ContentVariantSave> Variants { get; set; }

    [DataMember(Name = "contentTypeAlias", IsRequired = true)]
    [Required(AllowEmptyStrings = false)]
    public string ContentTypeAlias { get; set; } = null!;

    /// <summary>
    ///     The template alias to save
    /// </summary>
    [DataMember(Name = "templateAlias")]
    public string? TemplateAlias { get; set; }

    #region IContentSave

    [DataMember(Name = "action", IsRequired = true)]
    [Required]
    public ContentSaveAction Action { get; set; }

    [IgnoreDataMember]
    public List<ContentPropertyFile> UploadedFiles { get; }

    // These need explicit implementation because we are using internal models

    /// <inheritdoc />
    [IgnoreDataMember]
    IContent IContentSave<IContent>.PersistedContent { get; set; } = null!;

    // Non explicit internal getter so we don't need to explicitly cast in our own code
    [IgnoreDataMember]
    public IContent PersistedContent
    {
        get => ((IContentSave<IContent>)this).PersistedContent;
        set => ((IContentSave<IContent>)this).PersistedContent = value;
    }

    #endregion
}
