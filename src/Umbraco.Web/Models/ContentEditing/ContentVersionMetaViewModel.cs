using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "contentVersionMeta", Namespace = "")]
    public class ContentVersionMetaViewModel
    {
        [DataMember(Name = "contentId")]
        public int ContentId { get; set; }

        [DataMember(Name = "contentTypeId")]
        public int ContentTypeId { get; set; }

        [DataMember(Name = "versionId")]
        public int VersionId { get; set; }

        [DataMember(Name = "userId")]
        public int UserId { get; set; }

        [DataMember(Name = "versionDate")]
        public DateTime VersionDate { get; set; }

        [DataMember(Name = "currentPublishedVersion")]
        public bool CurrentPublishedVersion { get; set; }

        [DataMember(Name = "currentDraftVersion")]
        public bool CurrentDraftVersion { get; set; }

        [DataMember(Name = "preventCleanup")]
        public bool PreventCleanup { get; set; }

        [DataMember(Name = "username")]
        public string Username { get; set; }

        public ContentVersionMetaViewModel()
        {
        }

        public ContentVersionMetaViewModel(ContentVersionMeta dto)
        {
            ContentId = dto.ContentId;
            ContentTypeId = dto.ContentTypeId;
            VersionId = dto.VersionId;
            UserId = dto.UserId;
            VersionDate = dto.VersionDate;
            CurrentPublishedVersion = dto.CurrentPublishedVersion;
            CurrentDraftVersion = dto.CurrentDraftVersion;
            PreventCleanup = dto.PreventCleanup;
            Username = dto.Username;
        }
    }
}
