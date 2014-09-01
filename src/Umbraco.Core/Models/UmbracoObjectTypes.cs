using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Enum used to represent the Umbraco Object Types and thier associated GUIDs
    /// </summary>
    public enum UmbracoObjectTypes
    {
        /// <summary>
        /// Default value
        /// </summary>
        Unknown,

        /// <summary>
        /// Content Item Type
        /// </summary>
        [UmbracoObjectTypeAttribute(Constants.ObjectTypes.ContentItemType)]
        [FriendlyName("Content Item Type")]
        ContentItemType,

        /// <summary>
        /// Root
        /// </summary>
        [UmbracoObjectTypeAttribute(Constants.ObjectTypes.SystemRoot)]
        [FriendlyName("Root")]
        ROOT,

        /// <summary>
        /// Document
        /// </summary>
        [UmbracoObjectTypeAttribute(Constants.ObjectTypes.Document, typeof(IContent))]
        [FriendlyName("Document")]
        Document,

        /// <summary>
        /// Media
        /// </summary>
        [UmbracoObjectTypeAttribute(Constants.ObjectTypes.Media, typeof(IMedia))]
        [FriendlyName("Media")]
        Media,

        /// <summary>
        /// Member Type
        /// </summary>
        [UmbracoObjectTypeAttribute(Constants.ObjectTypes.MemberType, typeof(IMemberType))]
        [FriendlyName("Member Type")]
        MemberType,

        /// <summary>
        /// Template
        /// </summary>
        [UmbracoObjectTypeAttribute(Constants.ObjectTypes.Template, typeof(ITemplate))]
        [FriendlyName("Template")]
        Template,

        /// <summary>
        /// Member Group
        /// </summary>
        [UmbracoObjectTypeAttribute(Constants.ObjectTypes.MemberGroup)]
        [FriendlyName("Member Group")]
        MemberGroup,

        //TODO: What is a 'Content Item' supposed to be???
        /// <summary>
        /// Content Item
        /// </summary>
        [UmbracoObjectTypeAttribute(Constants.ObjectTypes.ContentItem)]
        [FriendlyName("Content Item")]
        ContentItem,

        /// <summary>
        /// "Media Type
        /// </summary>
        [UmbracoObjectTypeAttribute(Constants.ObjectTypes.MediaType, typeof(IMediaType))]
        [FriendlyName("Media Type")]
        MediaType,

        /// <summary>
        /// Document Type
        /// </summary>
        [UmbracoObjectTypeAttribute(Constants.ObjectTypes.DocumentType, typeof(IContentType))]
        [FriendlyName("Document Type")]
        DocumentType,

        /// <summary>
        /// Recycle Bin
        /// </summary>
        [UmbracoObjectTypeAttribute(Constants.ObjectTypes.ContentRecycleBin)]
        [FriendlyName("Recycle Bin")]
        RecycleBin,

        /// <summary>
        /// Stylesheet
        /// </summary>
        [UmbracoObjectTypeAttribute(Constants.ObjectTypes.Stylesheet)]
        [FriendlyName("Stylesheet")]
        Stylesheet,

        /// <summary>
        /// Member
        /// </summary>
        [UmbracoObjectTypeAttribute(Constants.ObjectTypes.Member, typeof(IMember))]
        [FriendlyName("Member")]
        Member,

        /// <summary>
        /// Data Type
        /// </summary>
        [UmbracoObjectTypeAttribute(Constants.ObjectTypes.DataType, typeof(IDataTypeDefinition))]
        [FriendlyName("Data Type")]
        DataType
    }
}