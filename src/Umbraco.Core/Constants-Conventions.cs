using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core
{
    public static partial class Constants
    {
        /// <summary>
        /// Defines the identifiers for property-type alias conventions that are used within the Umbraco core.
        /// </summary>
        public static class Conventions
        {
            internal static class PermissionCategories
            {
                public const string ContentCategory = "content";
                public const string AdministrationCategory = "administration";
                public const string StructureCategory = "structure";
                public const string OtherCategory = "other";
            }

            public static class PublicAccess
            {
                public const string MemberUsernameRuleType = "MemberUsername";
                public const string MemberRoleRuleType = "MemberRole";
            }


            public static class DataTypes
            {
                public const string ListViewPrefix = "List View - ";
            }

            /// <summary>
            /// Constants for Umbraco Content property aliases.
            /// </summary>
            public static class Content
            {
                /// <summary>
                /// Property alias for the Content's Url (internal) redirect.
                /// </summary>
                public const string InternalRedirectId = "umbracoInternalRedirectId";

                /// <summary>
                /// Property alias for the Content's navigational hide, (not actually used in core code).
                /// </summary>
                public const string NaviHide = "umbracoNaviHide";

                /// <summary>
                /// Property alias for the Content's Url redirect.
                /// </summary>
                public const string Redirect = "umbracoRedirect";

                /// <summary>
                /// Property alias for the Content's Url alias.
                /// </summary>
                public const string UrlAlias = "umbracoUrlAlias";

                /// <summary>
                /// Property alias for the Content's Url name.
                /// </summary>
                public const string UrlName = "umbracoUrlName";
            }

            /// <summary>
            /// Constants for Umbraco Media property aliases.
            /// </summary>
            public static class Media
            {
                /// <summary>
                /// Property alias for the Media's file name.
                /// </summary>
                public const string File = "umbracoFile";

                /// <summary>
                /// Property alias for the Media's width.
                /// </summary>
                public const string Width = "umbracoWidth";

                /// <summary>
                /// Property alias for the Media's height.
                /// </summary>
                public const string Height = "umbracoHeight";

                /// <summary>
                /// Property alias for the Media's file size (in bytes).
                /// </summary>
                public const string Bytes = "umbracoBytes";

                /// <summary>
                /// Property alias for the Media's file extension.
                /// </summary>
                public const string Extension = "umbracoExtension";

                /// <summary>
                /// The default height/width of an image file if the size can't be determined from the metadata
                /// </summary>
                public const int DefaultSize = 200;
            }

            /// <summary>
            /// Defines the alias identifiers for Umbraco media types.
            /// </summary>
            public static class MediaTypes
            {
                /// <summary>
                /// MediaType alias for a file.
                /// </summary>
                public const string File = "File";

                /// <summary>
                /// MediaType alias for a folder.
                /// </summary>
                public const string Folder = "Folder";

                /// <summary>
                /// MediaType alias for an image.
                /// </summary>
                public const string Image = "Image";

                /// <summary>
                /// MediaType name for a video.
                /// </summary>
                public const string Video = "Video";

                /// <summary>
                /// MediaType name for an audio.
                /// </summary>
                public const string Audio = "Audio";

                /// <summary>
                /// MediaType name for an article.
                /// </summary>
                public const string Article = "Article";

                /// <summary>
                /// MediaType name for vector graphics.
                /// </summary>
                public const string VectorGraphics = "VectorGraphics";

                /// <summary>
                /// MediaType alias for a video.
                /// </summary>
                public const string VideoAlias = "umbracoMediaVideo";

                /// <summary>
                /// MediaType alias for an audio.
                /// </summary>
                public const string AudioAlias = "umbracoMediaAudio";

                /// <summary>
                /// MediaType alias for an article.
                /// </summary>
                public const string ArticleAlias = "umbracoMediaArticle";

                /// <summary>
                /// MediaType alias for vector graphics.
                /// </summary>
                public const string VectorGraphicsAlias = "umbracoMediaVectorGraphics";

                /// <summary>
                /// MediaType alias indicating allowing auto-selection.
                /// </summary>
                public const string AutoSelect = "umbracoAutoSelect";
            }

            /// <summary>
            /// Constants for Umbraco Member property aliases.
            /// </summary>
            public static class Member
            {
                /// <summary>
                /// if a role starts with __umbracoRole we won't show it as it's an internal role used for public access
                /// </summary>
                public static readonly string InternalRolePrefix = "__umbracoRole";

                public static readonly string UmbracoMemberProviderName = "UmbracoMembershipProvider";

                public static readonly string UmbracoRoleProviderName = "UmbracoRoleProvider";

                /// <summary>
                /// Property alias for a Members Password Question
                /// </summary>
                public const string PasswordQuestion = "umbracoMemberPasswordRetrievalQuestion";

                public const string PasswordQuestionLabel = "Password Question";

                /// <summary>
                /// Property alias for Members Password Answer
                /// </summary>
                public const string PasswordAnswer = "umbracoMemberPasswordRetrievalAnswer";

                public const string PasswordAnswerLabel = "Password Answer";

                /// <summary>
                /// Property alias for the Comments on a Member
                /// </summary>
                public const string Comments = "umbracoMemberComments";

                public const string CommentsLabel = "Comments";

                /// <summary>
                /// Property alias for the Approved boolean of a Member
                /// </summary>
                public const string IsApproved = "umbracoMemberApproved";

                public const string IsApprovedLabel = "Is Approved";

                /// <summary>
                /// Property alias for the Locked out boolean of a Member
                /// </summary>
                public const string IsLockedOut = "umbracoMemberLockedOut";

                public const string IsLockedOutLabel = "Is Locked Out";

                /// <summary>
                /// Property alias for the last date the Member logged in
                /// </summary>
                public const string LastLoginDate = "umbracoMemberLastLogin";

                public const string LastLoginDateLabel = "Last Login Date";

                /// <summary>
                /// Property alias for the last date a Member changed its password
                /// </summary>
                public const string LastPasswordChangeDate = "umbracoMemberLastPasswordChangeDate";

                public const string LastPasswordChangeDateLabel = "Last Password Change Date";

                /// <summary>
                /// Property alias for the last date a Member was locked out
                /// </summary>
                public const string LastLockoutDate = "umbracoMemberLastLockoutDate";

                public const string LastLockoutDateLabel = "Last Lockout Date";

                /// <summary>
                /// Property alias for the number of failed login attempts
                /// </summary>
                public const string FailedPasswordAttempts = "umbracoMemberFailedPasswordAttempts";

                public const string FailedPasswordAttemptsLabel = "Failed Password Attempts";

                /// <summary>
                /// The standard properties group alias for membership properties.
                /// </summary>
                public const string StandardPropertiesGroupAlias = "membership";

                /// <summary>
                /// The standard properties group name for membership properties.
                /// </summary>
                public const string StandardPropertiesGroupName = "Membership";

                public static Dictionary<string, PropertyType> GetStandardPropertyTypeStubs()
                {
                    return new Dictionary<string, PropertyType>
                    {
                        {
                            Comments,
                            new PropertyType(PropertyEditors.Aliases.TextArea, ValueStorageType.Ntext, true, Comments)
                            {
                                Name = CommentsLabel
                            }
                        },
                        {
                            FailedPasswordAttempts,
                            new PropertyType(PropertyEditors.Aliases.Label, ValueStorageType.Integer, true, FailedPasswordAttempts)
                            {
                                Name = FailedPasswordAttemptsLabel,
                                DataTypeId = Constants.DataTypes.LabelInt
                            }
                        },
                        {
                            IsApproved,
                            new PropertyType(PropertyEditors.Aliases.Boolean, ValueStorageType.Integer, true, IsApproved)
                            {
                                Name = IsApprovedLabel
                            }
                        },
                        {
                            IsLockedOut,
                            new PropertyType(PropertyEditors.Aliases.Boolean, ValueStorageType.Integer, true, IsLockedOut)
                            {
                                Name = IsLockedOutLabel
                            }
                        },
                        {
                            LastLockoutDate,
                            new PropertyType(PropertyEditors.Aliases.Label, ValueStorageType.Date, true, LastLockoutDate)
                            {
                                Name = LastLockoutDateLabel,
                                DataTypeId = Constants.DataTypes.LabelDateTime
                            }
                        },
                        {
                            LastLoginDate,
                            new PropertyType(PropertyEditors.Aliases.Label, ValueStorageType.Date, true, LastLoginDate)
                            {
                                Name = LastLoginDateLabel,
                                DataTypeId = Constants.DataTypes.LabelDateTime
                            }
                        },
                        {
                            LastPasswordChangeDate,
                            new PropertyType(PropertyEditors.Aliases.Label, ValueStorageType.Date, true, LastPasswordChangeDate)
                            {
                                Name = LastPasswordChangeDateLabel,
                                DataTypeId = Constants.DataTypes.LabelDateTime
                            }
                        },
                        {
                            PasswordAnswer,
                            new PropertyType(PropertyEditors.Aliases.Label, ValueStorageType.Nvarchar, true, PasswordAnswer)
                            {
                                Name = PasswordAnswerLabel,
                                DataTypeId = Constants.DataTypes.LabelString
                            }
                        },
                        {
                            PasswordQuestion,
                            new PropertyType(PropertyEditors.Aliases.Label, ValueStorageType.Nvarchar, true, PasswordQuestion)
                            {
                                Name = PasswordQuestionLabel,
                                DataTypeId = Constants.DataTypes.LabelString
                            }
                        }
                    };
                }
            }

            /// <summary>
            /// Defines the alias identifiers for Umbraco member types.
            /// </summary>
            public static class MemberTypes
            {
                /// <summary>
                /// MemberType alias for default member type.
                /// </summary>
                public const string DefaultAlias = "Member";

                public const string SystemDefaultProtectType = "_umbracoSystemDefaultProtectType";

                public const string AllMembersListId = "all-members";
            }

            /// <summary>
            /// Constants for Umbraco URLs/Querystrings.
            /// </summary>
            public static class Url
            {
                /// <summary>
                /// Querystring parameter name used for Umbraco's alternative template functionality.
                /// </summary>
                public const string AltTemplate = "altTemplate";
            }

            /// <summary>
            /// Defines the alias identifiers for built-in Umbraco relation types.
            /// </summary>
            public static class RelationTypes
            {
                /// <summary>
                /// Name for default relation type "Related Media".
                /// </summary>
                public const string RelatedMediaName = "Related Media";

                /// <summary>
                /// Alias for default relation type "Related Media"
                /// </summary>
                public const string RelatedMediaAlias = "umbMedia";

                /// <summary>
                /// Name for default relation type "Related Document".
                /// </summary>
                public const string RelatedDocumentName = "Related Document";

                /// <summary>
                /// Alias for default relation type "Related Document"
                /// </summary>
                public const string RelatedDocumentAlias = "umbDocument";

                /// <summary>
                /// Name for default relation type "Relate Document On Copy".
                /// </summary>
                public const string RelateDocumentOnCopyName = "Relate Document On Copy";

                /// <summary>
                /// Alias for default relation type "Relate Document On Copy".
                /// </summary>
                public const string RelateDocumentOnCopyAlias = "relateDocumentOnCopy";

                /// <summary>
                /// Name for default relation type "Relate Parent Document On Delete".
                /// </summary>
                public const string RelateParentDocumentOnDeleteName = "Relate Parent Document On Delete";

                /// <summary>
                /// Alias for default relation type "Relate Parent Document On Delete".
                /// </summary>
                public const string RelateParentDocumentOnDeleteAlias = "relateParentDocumentOnDelete";

                /// <summary>
                /// Name for default relation type "Relate Parent Media Folder On Delete".
                /// </summary>
                public const string RelateParentMediaFolderOnDeleteName = "Relate Parent Media Folder On Delete";

                /// <summary>
                /// Alias for default relation type "Relate Parent Media Folder On Delete".
                /// </summary>
                public const string RelateParentMediaFolderOnDeleteAlias = "relateParentMediaFolderOnDelete";

                /// <summary>
                /// Returns the types of relations that are automatically tracked
                /// </summary>
                /// <remarks>
                /// Developers should not manually use these relation types since they will all be cleared whenever an entity
                /// (content, media or member) is saved since they are auto-populated based on property values.
                /// </remarks>
                public static string[] AutomaticRelationTypes = new[] { RelatedMediaAlias, RelatedDocumentAlias };

                //TODO: return a list of built in types so we can use that to prevent deletion in the uI
            }
        }
    }
}
