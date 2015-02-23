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
		    public static class PublicAccess
		    {
                public const string MemberUsernameRuleType = "MemberUsername";               
                public const string MemberRoleRuleType = "MemberRole";

                [Obsolete("No longer supported, this is here for backwards compatibility only")]
                public const string MemberIdRuleType = "MemberId";
                [Obsolete("No longer supported, this is here for backwards compatibility only")]
                public const string MemberGroupIdRuleType = "MemberGroupId";
		    }

		    public static class Localization
		    {
                /// <summary>
                /// The root id for all top level dictionary items
                /// </summary>
                public const string DictionaryItemRootId = "41c7638d-f529-4bff-853e-59a0c2fb1bde";
		    }

		    public static class DataTypes
		    {
		        public const string ListViewPrefix = "List View - ";
		    }

		    public static class PropertyGroups
		    {
		        public const string ListViewGroupName = "umbContainerView";
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
                /// Property alias for the number of failed login attemps
                /// </summary>
                public const string FailedPasswordAttempts = "umbracoMemberFailedPasswordAttempts";

                public const string FailedPasswordAttemptsLabel = "Failed Password Attempts";

                /// <summary>
                /// Group name to put the membership properties on
                /// </summary>
                internal const string StandardPropertiesGroupName = "Membership";

		        internal static Dictionary<string, PropertyType> GetStandardPropertyTypeStubs()
		        {
		            return new Dictionary<string, PropertyType>
		                {
		                    {
		                        Comments,
		                        new PropertyType(PropertyEditors.TextboxMultipleAlias, DataTypeDatabaseType.Ntext, true)
		                            {
		                                Alias = Comments,
		                                Name = CommentsLabel
		                            }
		                    },
		                    {
		                        FailedPasswordAttempts,
		                        new PropertyType(PropertyEditors.NoEditAlias, DataTypeDatabaseType.Integer, true)
		                            {
		                                Alias = FailedPasswordAttempts,
		                                Name = FailedPasswordAttemptsLabel
		                            }
		                    },
		                    {
		                        IsApproved,
		                        new PropertyType(PropertyEditors.TrueFalseAlias, DataTypeDatabaseType.Integer, true)
		                            {
		                                Alias = IsApproved,
		                                Name = IsApprovedLabel
		                            }
		                    },
		                    {
		                        IsLockedOut,
		                        new PropertyType(PropertyEditors.TrueFalseAlias, DataTypeDatabaseType.Integer, true)
		                            {
		                                Alias = IsLockedOut,
		                                Name = IsLockedOutLabel
		                            }
		                    },
		                    {
		                        LastLockoutDate,
		                        new PropertyType(PropertyEditors.NoEditAlias, DataTypeDatabaseType.Date, true)
		                            {
		                                Alias = LastLockoutDate,
		                                Name = LastLockoutDateLabel
		                            }
		                    },
		                    {
		                        LastLoginDate,
		                        new PropertyType(PropertyEditors.NoEditAlias, DataTypeDatabaseType.Date, true)
		                            {
		                                Alias = LastLoginDate,
		                                Name = LastLoginDateLabel
		                            }
		                    },
		                    {
		                        LastPasswordChangeDate,
		                        new PropertyType(PropertyEditors.NoEditAlias, DataTypeDatabaseType.Date, true)
		                            {
		                                Alias = LastPasswordChangeDate,
		                                Name = LastPasswordChangeDateLabel
		                            }
		                    },
		                    {
		                        PasswordAnswer,
		                        new PropertyType(PropertyEditors.NoEditAlias, DataTypeDatabaseType.Nvarchar, true)
		                            {
		                                Alias = PasswordAnswer,
		                                Name = PasswordAnswerLabel
		                            }
		                    },
		                    {
		                        PasswordQuestion,
		                        new PropertyType(PropertyEditors.NoEditAlias, DataTypeDatabaseType.Nvarchar, true)
		                            {
		                                Alias = PasswordQuestion,
		                                Name = PasswordQuestionLabel
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
                /// ContentType name for default relation type "Relate Document On Copy".
                /// </summary>
                public const string RelateDocumentOnCopyName = "Relate Document On Copy";
                
                /// <summary>
                /// ContentType alias for default relation type "Relate Document On Copy".
                /// </summary>
                public const string RelateDocumentOnCopyAlias = "relateDocumentOnCopy";

                /// <summary>
                /// ContentType name for default relation type "Relate Parent Document On Delete".
                /// </summary>
                public const string RelateParentDocumentOnDeleteName = "Relate Parent Document On Delete";

                /// <summary>
                /// ContentType alias for default relation type "Relate Parent Document On Delete".
                /// </summary>
                public const string RelateParentDocumentOnDeleteAlias = "relateParentDocumentOnDelete";
            }
		}
	}
}