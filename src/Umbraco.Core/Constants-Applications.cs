﻿namespace Umbraco.Core
{
	public static partial class Constants
	{
		/// <summary>
		/// Defines the alias identifiers for Umbraco's core application sections.
		/// </summary>
		public static class Applications
		{
			/// <summary>
			/// Application alias for the content section.
			/// </summary>
			public const string Content = "content";

			/// <summary>
			/// Application alias for the developer section.
			/// </summary>
			public const string Developer = "developer";

			/// <summary>
			/// Application alias for the media section.
			/// </summary>
			public const string Media = "media";

			/// <summary>
			/// Application alias for the members section.
			/// </summary>
			public const string Members = "member";

			/// <summary>
			/// Application alias for the settings section.
			/// </summary>
			public const string Settings = "settings";

			/// <summary>
			/// Application alias for the translation section.
			/// </summary>
			public const string Translation = "translation";

			/// <summary>
			/// Application alias for the users section.
			/// </summary>
			public const string Users = "users";

            /// <summary>
            /// Application alias for the forms section.
            /// </summary>
            public const string Forms = "forms";
		}

        /// <summary>
        /// Defines the alias identifiers for Umbraco's core trees.
        /// </summary>
        public static class Trees
        {
            /// <summary>
            /// alias for the content tree.
            /// </summary>
            public const string Content = "content";

            /// <summary>
            /// alias for the content blueprint tree.
            /// </summary>
            public const string ContentBlueprints = "contentBlueprints";

            /// <summary>
            /// alias for the member tree.
            /// </summary>
            public const string Members = "member";

            /// <summary>
            /// alias for the media tree.
            /// </summary>
            public const string Media = "media";

            /// <summary>
            /// alias for the macro tree.
            /// </summary>
            public const string Macros = "macros";
            
            /// <summary>
            /// alias for the datatype tree.
            /// </summary>
			public const string DataTypes = "dataTypes";

            /// <summary>
            /// alias for the packages tree
            /// </summary>
            public const string Packages = "packager";

			/// <summary>
			/// alias for the dictionary tree.
			/// </summary>
			public const string Dictionary = "dictionary";
            
            public const string Stylesheets = "stylesheets";

            /// <summary>
            /// alias for the document type tree.
            /// </summary>
            public const string DocumentTypes = "documentTypes";

            /// <summary>
            /// alias for the media type tree.
            /// </summary>
            public const string MediaTypes = "mediaTypes";


            /// <summary>
            /// alias for the member type tree.
            /// </summary>
            public const string MemberTypes = "memberTypes";

            /// <summary>
            /// alias for the template tree.
            /// </summary>
            public const string Templates = "templates";

            public const string RelationTypes = "relationTypes";

            public const string Xslt = "xslt";

            public const string Languages = "languages";

            public const string PartialViews = "partialViews";

            public const string PartialViewMacros = "partialViewMacros";

            public const string Scripts = "scripts";

            public const string Users = "users";

            //TODO: Fill in the rest!
        }
    }

   
}
