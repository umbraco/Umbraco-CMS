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
        [UmbracoObjectType(Constants.ObjectTypes.ContentItemType)]
        [FriendlyName("Content Item Type")]
        ContentItemType,

        /// <summary>
        /// Root
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.SystemRoot)]
        [FriendlyName("Root")]
        ROOT,

        /// <summary>
        /// Document
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.Document, typeof(IContent))]
        [FriendlyName("Document")]
        Document,

        /// <summary>
        /// Media
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.Media, typeof(IMedia))]
        [FriendlyName("Media")]
        Media,

        /// <summary>
        /// Member Type
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.MemberType, typeof(IMemberType))]
        [FriendlyName("Member Type")]
        MemberType,

        /// <summary>
        /// Template
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.Template, typeof(ITemplate))]
        [FriendlyName("Template")]
        Template,

        /// <summary>
        /// Member Group
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.MemberGroup)]
        [FriendlyName("Member Group")]
        MemberGroup,

        //TODO: What is a 'Content Item' supposed to be???
        /// <summary>
        /// Content Item
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.ContentItem)]
        [FriendlyName("Content Item")]
        ContentItem,

        /// <summary>
        /// "Media Type
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.MediaType, typeof(IMediaType))]
        [FriendlyName("Media Type")]
        MediaType,

        /// <summary>
        /// Document Type
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.DocumentType, typeof(IContentType))]
        [FriendlyName("Document Type")]
        DocumentType,

        /// <summary>
        /// Recycle Bin
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.ContentRecycleBin)]
        [FriendlyName("Recycle Bin")]
        RecycleBin,

        /// <summary>
        /// Stylesheet
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.Stylesheet)]
        [FriendlyName("Stylesheet")]
        Stylesheet,

        /// <summary>
        /// Member
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.Member, typeof(IMember))]
        [FriendlyName("Member")]
        Member,

        /// <summary>
        /// Data Type
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.DataType, typeof(IDataTypeDefinition))]
        [FriendlyName("Data Type")]
        DataType,

        /// <summary>
        /// Document type container
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.DocumentTypeContainer)]
        [FriendlyName("Document Type Container")]
        DocumentTypeContainer,

        /// <summary>
        /// Media type container
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.MediaTypeContainer)]
        [FriendlyName("Media Type Container")]
        MediaTypeContainer,

        /// <summary>
        /// Media type container
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.DataTypeContainer)]
        [FriendlyName("Data Type Container")]
        DataTypeContainer


    }
}