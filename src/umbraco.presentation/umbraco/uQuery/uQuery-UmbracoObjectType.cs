using System;

namespace umbraco
{
	public static partial class uQuery
	{
		/// <summary>
		/// Enum used to represent the Umbraco Object Types and thier associated GUIDs
		/// </summary>
		public enum UmbracoObjectType
		{
			/// <summary>
			/// Default value
			/// </summary>
			Unknown,

			/// <summary>
			/// Content Item Type
			/// </summary>
			[Guid("7A333C54-6F43-40A4-86A2-18688DC7E532")]
			[FriendlyName("Content Item Type")]
			ContentItemType,

			/// <summary>
			/// Root
			/// </summary>
			[Guid("EA7D8624-4CFE-4578-A871-24AA946BF34D")]
			[FriendlyName("Root")]
			ROOT,

			/// <summary>
			/// Document
			/// </summary>
			[Guid("C66BA18E-EAF3-4CFF-8A22-41B16D66A972")]
			[FriendlyName("Document")]
			Document,

			/// <summary>
			/// Media
			/// </summary>
			[Guid("B796F64C-1F99-4FFB-B886-4BF4BC011A9C")]
			[FriendlyName("Media")]
			Media,

			/// <summary>
			/// Member Type
			/// </summary>
			[Guid("9B5416FB-E72F-45A9-A07B-5A9A2709CE43")]
			[FriendlyName("Member Type")]
			MemberType,

			/// <summary>
			/// Template
			/// </summary>
			[Guid("6FBDE604-4178-42CE-A10B-8A2600A2F07D")]
			[FriendlyName("Template")]
			Template,

			/// <summary>
			/// Member Group
			/// </summary>
			[Guid("366E63B9-880F-4E13-A61C-98069B029728")]
			[FriendlyName("Member Group")]
			MemberGroup,

			/// <summary>
			/// Content Item
			/// </summary>
			[Guid("10E2B09F-C28B-476D-B77A-AA686435E44A")]
			[FriendlyName("Content Item")]
			ContentItem,

			/// <summary>
			/// "Media Type
			/// </summary>
			[Guid("4EA4382B-2F5A-4C2B-9587-AE9B3CF3602E")]
			[FriendlyName("Media Type")]
			MediaType,

			/// <summary>
			/// Document Type
			/// </summary>
			[Guid("A2CB7800-F571-4787-9638-BC48539A0EFB")]
			[FriendlyName("Document Type")]
			DocumentType,

			/// <summary>
			/// Recycle Bin
			/// </summary>
			[Guid("01BB7FF2-24DC-4C0C-95A2-C24EF72BBAC8")]
			[FriendlyName("Recycle Bin")]
			RecycleBin,

			/// <summary>
			/// Stylesheet
			/// </summary>
			[Guid("9F68DA4F-A3A8-44C2-8226-DCBD125E4840")]
			[FriendlyName("Stylesheet")]
			Stylesheet,

			/// <summary>
			/// Member
			/// </summary>
			[Guid("39EB0F98-B348-42A1-8662-E7EB18487560")]
			[FriendlyName("Member")]
			Member,

			/// <summary>
			/// Data Type
			/// </summary>
			[Guid("30A2A501-1978-4DDB-A57B-F7EFED43BA3C")]
			[FriendlyName("Data Type")]
			DataType
		}

		/// <summary>
		/// Get an UmbracoObjectType value from it's name
		/// </summary>
		/// <param name="name">Enum value name</param>
		/// <returns>an UmbracoObjectType Enum value</returns>
		public static UmbracoObjectType GetUmbracoObjectType(string name)
		{
			return (UmbracoObjectType)Enum.Parse(typeof(UmbracoObjectType), name, false);
		}

		/// <summary>
		/// Get an instance of an Umbraco Object Type enum value from it's GUID
		/// </summary>
		/// <param name="guid">Enum value GUID</param>
		/// <returns>an UmbracoObjectType Enum value</returns>
		public static UmbracoObjectType GetUmbracoObjectType(Guid guid)
		{
			var umbracoObjectType = UmbracoObjectType.Unknown;

			foreach (var name in Enum.GetNames(typeof(UmbracoObjectType)))
			{
				if (uQuery.GetUmbracoObjectType(name).GetGuid() == guid)
				{
					umbracoObjectType = GetUmbracoObjectType(name);
				}
			}

			return umbracoObjectType;
		}

		/// <summary>
		/// Get an UmbracoObjectType value from an id in umbracoNode table
		/// </summary>
		/// <param name="id">id to search for</param>
		/// <returns>an UmbracoObjectType Enum value</returns>
		public static UmbracoObjectType GetUmbracoObjectType(int id)
		{
			return uQuery.GetUmbracoObjectType(
						uQuery.SqlHelper.ExecuteScalar<Guid>(
							string.Concat("SELECT nodeObjectType FROM umbracoNode WHERE id = ", id)));
		}

		/// <summary>
		/// Attribute to associate a GUID string with an UmbracoObjectType Enum value
		/// </summary>
		internal class GuidAttribute : Attribute
		{
			/// <summary>
			/// string representation of a Guid
			/// </summary>
			private readonly string guid;

			/// <summary>
			/// Initializes a new instance of the GuidAttribute class
			/// Sets the guid value as a string
			/// </summary>
			/// <param name="guid">a string representation of a Guid from the enum attribute</param>
			public GuidAttribute(string guid)
			{
				this.guid = guid;
			}

			/// <summary>
			/// Gets the guid as a string
			/// </summary>
			/// <returns>string of guid</returns>
			public override string ToString()
			{
				return this.guid;
			}
		}

		/// <summary>
		/// Attribute to add a Friendly Name string with an UmbracoObjectType enum value
		/// </summary>
		internal class FriendlyNameAttribute : Attribute
		{
			/// <summary>
			/// friendly name value
			/// </summary>
			private readonly string friendlyName;

			/// <summary>
			/// Initializes a new instance of the FriendlyNameAttribute class
			/// Sets the friendly name value
			/// </summary>
			/// <param name="friendlyName">attribute value</param>
			public FriendlyNameAttribute(string friendlyName)
			{
				this.friendlyName = friendlyName;
			}

			/// <summary>
			/// Gets the friendly name
			/// </summary>
			/// <returns>string of friendly name</returns>
			public override string ToString()
			{
				return this.friendlyName;
			}
		}
	}
}