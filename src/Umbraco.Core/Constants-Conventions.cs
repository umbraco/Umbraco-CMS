namespace Umbraco.Core
{
	public static partial class Constants
	{
		/// <summary>
		/// Defines the identifiers for property-type alias conventions that are used within the Umbraco core.
		/// </summary>
		public static class Conventions
		{
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
			/// Constants for Umbraco URLs/Querystrings.
			/// </summary>
			public static class Url
			{
				/// <summary>
				/// Querystring parameter name used for Umbraco's alternative template functionality.
				/// </summary>
				public const string AltTemplate = "altTemplate";
			}
		}
	}
}