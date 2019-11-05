using System;
using System.ComponentModel;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Enum used to represent the Umbraco Object Types and their associated GUIDs
    /// </summary>
    public enum UmbracoObjectTypes
    {
        /// <summary>
        /// Default value
        /// </summary>
        Unknown,


        /// <summary>
        /// Root
        /// </summary>
        [UmbracoObjectType(ConstantsCore.ObjectTypes.Strings.SystemRoot)]
        [FriendlyName("Root")]
        ROOT,

        /// <summary>
        /// Document
        /// </summary>
        [UmbracoObjectType(ConstantsCore.ObjectTypes.Strings.Document, typeof(IContent))]
        [FriendlyName("Document")]
        [UmbracoUdiType(Constants.UdiEntityType.Document)]
        Document,

        /// <summary>
        /// Media
        /// </summary>
        [UmbracoObjectType(ConstantsCore.ObjectTypes.Strings.Media, typeof(IMedia))]
        [FriendlyName("Media")]
        [UmbracoUdiType(Constants.UdiEntityType.Media)]
        Media,

        /// <summary>
        /// Member Type
        /// </summary>
        [UmbracoObjectType(ConstantsCore.ObjectTypes.Strings.MemberType, typeof(IMemberType))]
        [FriendlyName("Member Type")]
        [UmbracoUdiType(Constants.UdiEntityType.MemberType)]
        MemberType,

        /// <summary>
        /// Template
        /// </summary>
        [UmbracoObjectType(ConstantsCore.ObjectTypes.Strings.Template, typeof(ITemplate))]
        [FriendlyName("Template")]
        [UmbracoUdiType(Constants.UdiEntityType.Template)]
        Template,

        /// <summary>
        /// Member Group
        /// </summary>
        [UmbracoObjectType(ConstantsCore.ObjectTypes.Strings.MemberGroup)]
        [FriendlyName("Member Group")]
        [UmbracoUdiType(Constants.UdiEntityType.MemberGroup)]
        MemberGroup,

        /// <summary>
        /// "Media Type
        /// </summary>
        [UmbracoObjectType(ConstantsCore.ObjectTypes.Strings.MediaType, typeof(IMediaType))]
        [FriendlyName("Media Type")]
        [UmbracoUdiType(Constants.UdiEntityType.MediaType)]
        MediaType,

        /// <summary>
        /// Document Type
        /// </summary>
        [UmbracoObjectType(ConstantsCore.ObjectTypes.Strings.DocumentType, typeof(IContentType))]
        [FriendlyName("Document Type")]
        [UmbracoUdiType(Constants.UdiEntityType.DocumentType)]
        DocumentType,

        /// <summary>
        /// Recycle Bin
        /// </summary>
        [UmbracoObjectType(ConstantsCore.ObjectTypes.Strings.ContentRecycleBin)]
        [FriendlyName("Recycle Bin")]
        RecycleBin,

        /// <summary>
        /// Stylesheet
        /// </summary>
        [UmbracoObjectType(ConstantsCore.ObjectTypes.Strings.Stylesheet)]
        [FriendlyName("Stylesheet")]
        [UmbracoUdiType(Constants.UdiEntityType.Stylesheet)]
        Stylesheet,

        /// <summary>
        /// Member
        /// </summary>
        [UmbracoObjectType(ConstantsCore.ObjectTypes.Strings.Member, typeof(IMember))]
        [FriendlyName("Member")]
        [UmbracoUdiType(Constants.UdiEntityType.Member)]
        Member,

        /// <summary>
        /// Data Type
        /// </summary>
        [UmbracoObjectType(ConstantsCore.ObjectTypes.Strings.DataType, typeof(IDataType))]
        [FriendlyName("Data Type")]
        [UmbracoUdiType(Constants.UdiEntityType.DataType)]
        DataType,

        /// <summary>
        /// Document type container
        /// </summary>
        [UmbracoObjectType(ConstantsCore.ObjectTypes.Strings.DocumentTypeContainer)]
        [FriendlyName("Document Type Container")]
        [UmbracoUdiType(Constants.UdiEntityType.DocumentTypeContainer)]
        DocumentTypeContainer,

        /// <summary>
        /// Media type container
        /// </summary>
        [UmbracoObjectType(ConstantsCore.ObjectTypes.Strings.MediaTypeContainer)]
        [FriendlyName("Media Type Container")]
        [UmbracoUdiType(Constants.UdiEntityType.MediaTypeContainer)]
        MediaTypeContainer,

        /// <summary>
        /// Media type container
        /// </summary>
        [UmbracoObjectType(ConstantsCore.ObjectTypes.Strings.DataTypeContainer)]
        [FriendlyName("Data Type Container")]
        [UmbracoUdiType(Constants.UdiEntityType.DataTypeContainer)]
        DataTypeContainer,

        /// <summary>
        /// Relation type
        /// </summary>
        [UmbracoObjectType(ConstantsCore.ObjectTypes.Strings.RelationType)]
        [FriendlyName("Relation Type")]
        [UmbracoUdiType(Constants.UdiEntityType.RelationType)]
        RelationType,

        /// <summary>
        /// Forms Form
        /// </summary>
        [UmbracoObjectType(ConstantsCore.ObjectTypes.Strings.FormsForm)]
        [FriendlyName("Form")]
        FormsForm,

        /// <summary>
        /// Forms PreValue
        /// </summary>
        [UmbracoObjectType(ConstantsCore.ObjectTypes.Strings.FormsPreValue)]
        [FriendlyName("PreValue")]
        FormsPreValue,

        /// <summary>
        /// Forms DataSource
        /// </summary>
        [UmbracoObjectType(ConstantsCore.ObjectTypes.Strings.FormsDataSource)]
        [FriendlyName("DataSource")]
        FormsDataSource,

        /// <summary>
        /// Language
        /// </summary>
        [UmbracoObjectType(ConstantsCore.ObjectTypes.Strings.Language)]
        [FriendlyName("Language")]
        Language,

        /// <summary>
        /// Document Blueprint
        /// </summary>
        [UmbracoObjectType(ConstantsCore.ObjectTypes.Strings.DocumentBlueprint, typeof(IContent))]
        [FriendlyName("DocumentBlueprint")]
        [UmbracoUdiType(Constants.UdiEntityType.DocumentBlueprint)]
        DocumentBlueprint,

        /// <summary>
        /// Reserved Identifier
        /// </summary>
        [UmbracoObjectType(ConstantsCore.ObjectTypes.Strings.IdReservation)]
        [FriendlyName("Identifier Reservation")]
        IdReservation

    }
}
