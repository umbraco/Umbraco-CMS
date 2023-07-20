using Umbraco.Cms.Core.CodeAnnotations;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Enum used to represent the Umbraco Object Types and their associated GUIDs
/// </summary>
public enum UmbracoObjectTypes
{
    /// <summary>
    ///     Default value
    /// </summary>
    Unknown,

    /// <summary>
    ///     Root
    /// </summary>
    [UmbracoObjectType(Constants.ObjectTypes.Strings.SystemRoot)]
    [FriendlyName("Root")]
    ROOT,

    /// <summary>
    ///     Document
    /// </summary>
    [UmbracoObjectType(Constants.ObjectTypes.Strings.Document, typeof(IContent))]
    [FriendlyName("Document")]
    [UmbracoUdiType(Constants.UdiEntityType.Document)]
    Document,

    /// <summary>
    ///     Media
    /// </summary>
    [UmbracoObjectType(Constants.ObjectTypes.Strings.Media, typeof(IMedia))]
    [FriendlyName("Media")]
    [UmbracoUdiType(Constants.UdiEntityType.Media)]
    Media,

    /// <summary>
    ///     Member Type
    /// </summary>
    [UmbracoObjectType(Constants.ObjectTypes.Strings.MemberType, typeof(IMemberType))]
    [FriendlyName("Member Type")]
    [UmbracoUdiType(Constants.UdiEntityType.MemberType)]
    MemberType,

    /// <summary>
    ///     Template
    /// </summary>
    [UmbracoObjectType(Constants.ObjectTypes.Strings.Template, typeof(ITemplate))]
    [FriendlyName("Template")]
    [UmbracoUdiType(Constants.UdiEntityType.Template)]
    Template,

    /// <summary>
    ///     Member Group
    /// </summary>
    [UmbracoObjectType(Constants.ObjectTypes.Strings.MemberGroup, typeof(IMemberGroup))]
    [FriendlyName("Member Group")]
    [UmbracoUdiType(Constants.UdiEntityType.MemberGroup)]
    MemberGroup,

    /// <summary>
    ///     "Media Type
    /// </summary>
    [UmbracoObjectType(Constants.ObjectTypes.Strings.MediaType, typeof(IMediaType))]
    [FriendlyName("Media Type")]
    [UmbracoUdiType(Constants.UdiEntityType.MediaType)]
    MediaType,

    /// <summary>
    ///     Document Type
    /// </summary>
    [UmbracoObjectType(Constants.ObjectTypes.Strings.DocumentType, typeof(IContentType))]
    [FriendlyName("Document Type")]
    [UmbracoUdiType(Constants.UdiEntityType.DocumentType)]
    DocumentType,

    /// <summary>
    ///     Recycle Bin
    /// </summary>
    [UmbracoObjectType(Constants.ObjectTypes.Strings.ContentRecycleBin)]
    [FriendlyName("Recycle Bin")]
    RecycleBin,

    /// <summary>
    ///     Member
    /// </summary>
    [UmbracoObjectType(Constants.ObjectTypes.Strings.Member, typeof(IMember))]
    [FriendlyName("Member")]
    [UmbracoUdiType(Constants.UdiEntityType.Member)]
    Member,

    /// <summary>
    ///     Data Type
    /// </summary>
    [UmbracoObjectType(Constants.ObjectTypes.Strings.DataType, typeof(IDataType))]
    [FriendlyName("Data Type")]
    [UmbracoUdiType(Constants.UdiEntityType.DataType)]
    DataType,

    /// <summary>
    ///     Document type container
    /// </summary>
    [UmbracoObjectType(Constants.ObjectTypes.Strings.DocumentTypeContainer)]
    [FriendlyName("Document Type Container")]
    [UmbracoUdiType(Constants.UdiEntityType.DocumentTypeContainer)]
    DocumentTypeContainer,

    /// <summary>
    ///     Media type container
    /// </summary>
    [UmbracoObjectType(Constants.ObjectTypes.Strings.MediaTypeContainer)]
    [FriendlyName("Media Type Container")]
    [UmbracoUdiType(Constants.UdiEntityType.MediaTypeContainer)]
    MediaTypeContainer,

    /// <summary>
    ///     Media type container
    /// </summary>
    [UmbracoObjectType(Constants.ObjectTypes.Strings.DataTypeContainer)]
    [FriendlyName("Data Type Container")]
    [UmbracoUdiType(Constants.UdiEntityType.DataTypeContainer)]
    DataTypeContainer,

    /// <summary>
    ///     Relation type
    /// </summary>
    [UmbracoObjectType(Constants.ObjectTypes.Strings.RelationType)]
    [FriendlyName("Relation Type")]
    [UmbracoUdiType(Constants.UdiEntityType.RelationType)]
    RelationType,

    /// <summary>
    ///     Forms Form
    /// </summary>
    [UmbracoObjectType(Constants.ObjectTypes.Strings.FormsForm)]
    [FriendlyName("Form")]
    FormsForm,

    /// <summary>
    ///     Forms PreValue
    /// </summary>
    [UmbracoObjectType(Constants.ObjectTypes.Strings.FormsPreValue)]
    [FriendlyName("PreValue")]
    FormsPreValue,

    /// <summary>
    ///     Forms DataSource
    /// </summary>
    [UmbracoObjectType(Constants.ObjectTypes.Strings.FormsDataSource)]
    [FriendlyName("DataSource")]
    FormsDataSource,

    /// <summary>
    ///     Language
    /// </summary>
    [UmbracoObjectType(Constants.ObjectTypes.Strings.Language)]
    [FriendlyName("Language")]
    Language,

    /// <summary>
    ///     Document Blueprint
    /// </summary>
    [UmbracoObjectType(Constants.ObjectTypes.Strings.DocumentBlueprint, typeof(IContent))]
    [FriendlyName("DocumentBlueprint")]
    [UmbracoUdiType(Constants.UdiEntityType.DocumentBlueprint)]
    DocumentBlueprint,

    /// <summary>
    ///     Reserved Identifier
    /// </summary>
    [UmbracoObjectType(Constants.ObjectTypes.Strings.IdReservation)]
    [FriendlyName("Identifier Reservation")]
    IdReservation,
}
