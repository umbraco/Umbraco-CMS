namespace Umbraco.Cms.Core;

public partial class Constants
{
    public static class MediaTypes
    {
        /// <summary>
        /// Contains GUID constants for built-in media types.
        /// </summary>
        public static class Guids
        {
            /// <summary>
            /// The GUID for the Article media type as a string.
            /// </summary>
            public const string Article = "a43e3414-9599-4230-a7d3-943a21b20122";

            /// <summary>
            /// The GUID for the Audio media type as a string.
            /// </summary>
            public const string Audio = "a5ddeee0-8fd8-4cee-a658-6f1fcdb00de3";

            /// <summary>
            /// The GUID for the File media type as a string.
            /// </summary>
            public const string File = "4c52d8ab-54e6-40cd-999c-7a5f24903e4d";

            /// <summary>
            /// The GUID for the Image media type as a string.
            /// </summary>
            public const string Image = "cc07b313-0843-4aa8-bbda-871c8da728c8";

            /// <summary>
            /// The GUID for the SVG media type as a string.
            /// </summary>
            public const string Svg = "c4b1efcf-a9d5-41c4-9621-e9d273b52a9c";

            /// <summary>
            /// The GUID for the Video media type as a string.
            /// </summary>
            public const string Video = "f6c515bb-653c-4bdc-821c-987729ebe327";

            /// <summary>
            /// The GUID for the Article media type.
            /// </summary>
            public static readonly Guid ArticleGuid = new(Article);

            /// <summary>
            /// The GUID for the Audio media type.
            /// </summary>
            public static readonly Guid AudioGuid = new(Audio);

            /// <summary>
            /// The GUID for the File media type.
            /// </summary>
            public static readonly Guid FileGuid = new(File);

            /// <summary>
            /// The GUID for the Image media type.
            /// </summary>
            public static readonly Guid ImageGuid = new(Image);

            /// <summary>
            /// The GUID for the SVG media type.
            /// </summary>
            public static readonly Guid SvgGuid = new(Svg);

            /// <summary>
            /// The GUID for the Video media type.
            /// </summary>
            public static readonly Guid VideoGuid = new(Video);
        }
    }
}
