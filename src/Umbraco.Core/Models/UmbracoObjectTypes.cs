using System;
using System.ComponentModel;
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
        [Obsolete("This is not used and will be removed in future versions")]
        [EditorBrowsable(EditorBrowsableState.Never)]
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
        [UmbracoUdiType(Constants.UdiEntityType.Document)]
        Document,

        /// <summary>
        /// Media
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.Media, typeof(IMedia))]
        [FriendlyName("Media")]
        [UmbracoUdiType(Constants.UdiEntityType.Media)]
        Media,

        /// <summary>
        /// Member Type
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.MemberType, typeof(IMemberType))]
        [FriendlyName("Member Type")]
        [UmbracoUdiType(Constants.UdiEntityType.MemberType)]
        MemberType,

        /// <summary>
        /// Template
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.Template, typeof(ITemplate))]
        [FriendlyName("Template")]
        [UmbracoUdiType(Constants.UdiEntityType.Template)]
        Template,

        /// <summary>
        /// Member Group
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.MemberGroup)]
        [FriendlyName("Member Group")]
        [UmbracoUdiType(Constants.UdiEntityType.MemberGroup)]
        MemberGroup,

        //TODO: What is a 'Content Item' supposed to be???
        /// <summary>
        /// Content Item
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.ContentItem)]
        [FriendlyName("Content Item")]
        [Obsolete("This is not used and will be removed in future versions")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        ContentItem,

        /// <summary>
        /// "Media Type
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.MediaType, typeof(IMediaType))]
        [FriendlyName("Media Type")]
        [UmbracoUdiType(Constants.UdiEntityType.MediaType)]
        MediaType,

        /// <summary>
        /// Document Type
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.DocumentType, typeof(IContentType))]
        [FriendlyName("Document Type")]
        [UmbracoUdiType(Constants.UdiEntityType.DocumentType)]
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
        [UmbracoUdiType(Constants.UdiEntityType.Stylesheet)]
        Stylesheet,

        /// <summary>
        /// Member
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.Member, typeof(IMember))]
        [FriendlyName("Member")]
        [UmbracoUdiType(Constants.UdiEntityType.Member)]
        Member,

        /// <summary>
        /// Data Type
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.DataType, typeof(IDataTypeDefinition))]
        [FriendlyName("Data Type")]
        [UmbracoUdiType(Constants.UdiEntityType.DataType)]
        DataType,

        /// <summary>
        /// Document type container
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.DocumentTypeContainer)]
        [FriendlyName("Document Type Container")]
        [UmbracoUdiType(Constants.UdiEntityType.DocumentTypeContainer)]
        DocumentTypeContainer,

        /// <summary>
        /// Media type container
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.MediaTypeContainer)]
        [FriendlyName("Media Type Container")]
        [UmbracoUdiType(Constants.UdiEntityType.MediaTypeContainer)]
        MediaTypeContainer,

        /// <summary>
        /// Media type container
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.DataTypeContainer)]
        [FriendlyName("Data Type Container")]
        [UmbracoUdiType(Constants.UdiEntityType.DataTypeContainer)]
        DataTypeContainer,

        /// <summary>
        /// Relation type
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.RelationType)]
        [FriendlyName("Relation Type")]
        [UmbracoUdiType(Constants.UdiEntityType.RelationType)]
        RelationType,

        /// <summary>
        /// Forms Form
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.FormsForm)]
        [FriendlyName("Form")]
        FormsForm,

        /// <summary>
        /// Forms PreValue
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.FormsPreValue)]
        [FriendlyName("PreValue")]
        FormsPreValue,

        /// <summary>
        /// Forms DataSource
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.FormsDataSource)]
        [FriendlyName("DataSource")]
        FormsDataSource,

        /// <summary>
        /// Language
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.Language)]
        [FriendlyName("Language")]
        Language,

        /// <summary>
        /// Document Blueprint
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.DocumentBlueprint, typeof(IContent))]
        [FriendlyName("DocumentBlueprint")]
        [UmbracoUdiType(Constants.UdiEntityType.DocumentBluePrint)]
        DocumentBlueprint,
        
        /// <summary>
        /// Reserved Identifier
        /// </summary>
        [UmbracoObjectType(Constants.ObjectTypes.IdReservation)]
        [FriendlyName("Identifier Reservation")]
        IdReservation

    }
}