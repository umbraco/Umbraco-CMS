using System;

namespace Umbraco.Core
{
	public static partial class Constants
	{
		/// <summary>
		/// Defines the identifiers for Umbraco object types as constants for easy centralized access/management.
		/// </summary>
		public static class ObjectTypes
		{
			/// <summary>
			/// Guid for a Content Item object.
			/// </summary>
			public const string ContentItem = "10E2B09F-C28B-476D-B77A-AA686435E44A";

			/// <summary>
			/// Guid for a Content Item Type object.
			/// </summary>
			public const string ContentItemType = "7A333C54-6F43-40A4-86A2-18688DC7E532";

			/// <summary>
			/// Guid for the Content Recycle Bin.
			/// </summary>
			public const string ContentRecycleBin = "01BB7FF2-24DC-4C0C-95A2-C24EF72BBAC8";

			/// <summary>
			/// Guid for a DataType object.
			/// </summary>
			public const string DataType = "30A2A501-1978-4DDB-A57B-F7EFED43BA3C";

			/// <summary>
			/// Guid for a Document object.
			/// </summary>
			public const string Document = "C66BA18E-EAF3-4CFF-8A22-41B16D66A972";

			/// <summary>
			/// Guid for a Document Type object.
			/// </summary>
			public const string DocumentType = "A2CB7800-F571-4787-9638-BC48539A0EFB";

			/// <summary>
			/// Guid for a Media object.
			/// </summary>
			public const string Media = "B796F64C-1F99-4FFB-B886-4BF4BC011A9C";

			/// <summary>
			/// Guid for the Media Recycle Bin.
			/// </summary>
			public const string MediaRecycleBin = "CF3D8E34-1C1C-41e9-AE56-878B57B32113";

			/// <summary>
			/// Guid for a Media Type object.
			/// </summary>
			public const string MediaType = "4EA4382B-2F5A-4C2B-9587-AE9B3CF3602E";

			/// <summary>
			/// Guid for a Member object.
			/// </summary>
			public const string Member = "39EB0F98-B348-42A1-8662-E7EB18487560";

			/// <summary>
			/// Guid for a Member Group object.
			/// </summary>
			public const string MemberGroup = "366E63B9-880F-4E13-A61C-98069B029728";

			/// <summary>
			/// Guid for a Member Type object.
			/// </summary>
			public const string MemberType = "9B5416FB-E72F-45A9-A07B-5A9A2709CE43";

			/// <summary>
			/// Guid for a Stylesheet object.
			/// </summary>
            [Obsolete("This no longer exists in the database")]
            public const string Stylesheet = "9F68DA4F-A3A8-44C2-8226-DCBD125E4840";

            [Obsolete("This no longer exists in the database")]
            internal const string StylesheetProperty = "5555da4f-a123-42b2-4488-dcdfb25e4111";

			/// <summary>
			/// Guid for the System Root.
			/// </summary>
			public const string SystemRoot = "EA7D8624-4CFE-4578-A871-24AA946BF34D";

			/// <summary>
			/// Guid for a Template object.
			/// </summary>
			public const string Template = "6FBDE604-4178-42CE-A10B-8A2600A2F07D";
		}
	}
}