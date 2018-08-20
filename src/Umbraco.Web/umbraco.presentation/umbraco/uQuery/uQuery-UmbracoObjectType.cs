using System;
using umbraco.BusinessLogic;
using Umbraco.Core;

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
			[Guid(Constants.ObjectTypes.ContentItemType)]
			[FriendlyName("Content Item Type")]
			ContentItemType,

			/// <summary>
			/// Root
			/// </summary>
			[Guid(Constants.ObjectTypes.SystemRoot)]
			[FriendlyName("Root")]
			ROOT,

			/// <summary>
			/// Document
			/// </summary>
			[Guid(Constants.ObjectTypes.Document)]
			[FriendlyName("Document")]
			Document,

			/// <summary>
			/// Media
			/// </summary>
			[Guid(Constants.ObjectTypes.Media)]
			[FriendlyName("Media")]
			Media,

			/// <summary>
			/// Member Type
			/// </summary>
			[Guid(Constants.ObjectTypes.MemberType)]
			[FriendlyName("Member Type")]
			MemberType,

			/// <summary>
			/// Template
			/// </summary>
			[Guid(Constants.ObjectTypes.Template)]
			[FriendlyName("Template")]
			Template,

			/// <summary>
			/// Member Group
			/// </summary>
			[Guid(Constants.ObjectTypes.MemberGroup)]
			[FriendlyName("Member Group")]
			MemberGroup,

			/// <summary>
			/// Content Item
			/// </summary>
			[Guid(Constants.ObjectTypes.ContentItem)]
			[FriendlyName("Content Item")]
			ContentItem,

			/// <summary>
			/// "Media Type
			/// </summary>
			[Guid(Constants.ObjectTypes.MediaType)]
			[FriendlyName("Media Type")]
			MediaType,

			/// <summary>
			/// Document Type
			/// </summary>
			[Guid(Constants.ObjectTypes.DocumentType)]
			[FriendlyName("Document Type")]
			DocumentType,

			/// <summary>
			/// Recycle Bin
			/// </summary>
			[Guid(Constants.ObjectTypes.ContentRecycleBin)]
			[FriendlyName("Recycle Bin")]
			RecycleBin,

			/// <summary>
			/// Stylesheet
			/// </summary>
			[Guid(Constants.ObjectTypes.Stylesheet)]
			[FriendlyName("Stylesheet")]
			Stylesheet,

			/// <summary>
			/// Member
			/// </summary>
			[Guid(Constants.ObjectTypes.Member)]
			[FriendlyName("Member")]
			Member,

			/// <summary>
			/// Data Type
			/// </summary>
			[Guid(Constants.ObjectTypes.DataType)]
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
		    using (var sqlHelper = Application.SqlHelper)
		    {
		        var guid = sqlHelper.ExecuteScalar<Guid>(string.Concat("SELECT nodeObjectType FROM umbracoNode WHERE id = ", id));
		        return GetUmbracoObjectType(guid);
		    }
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