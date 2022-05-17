namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Defines the Umbraco object type unique identifiers.
    /// </summary>
    public static class ObjectTypes
    {
        public static readonly Guid SystemRoot = new(Strings.SystemRoot);

        public static readonly Guid ContentRecycleBin = new(Strings.ContentRecycleBin);

        public static readonly Guid MediaRecycleBin = new(Strings.MediaRecycleBin);

        public static readonly Guid DataTypeContainer = new(Strings.DataTypeContainer);

        public static readonly Guid DocumentTypeContainer = new(Strings.DocumentTypeContainer);

        public static readonly Guid MediaTypeContainer = new(Strings.MediaTypeContainer);

        public static readonly Guid DataType = new(Strings.DataType);

        public static readonly Guid Document = new(Strings.Document);

        public static readonly Guid DocumentBlueprint = new(Strings.DocumentBlueprint);

        public static readonly Guid DocumentType = new(Strings.DocumentType);

        public static readonly Guid Media = new(Strings.Media);

        public static readonly Guid MediaType = new(Strings.MediaType);

        public static readonly Guid Member = new(Strings.Member);

        public static readonly Guid MemberGroup = new(Strings.MemberGroup);

        public static readonly Guid MemberType = new(Strings.MemberType);

        public static readonly Guid TemplateType = new(Strings.Template);

        public static readonly Guid LockObject = new(Strings.LockObject);

        public static readonly Guid RelationType = new(Strings.RelationType);

        public static readonly Guid FormsForm = new(Strings.FormsForm);

        public static readonly Guid FormsPreValue = new(Strings.FormsPreValue);

        public static readonly Guid FormsDataSource = new(Strings.FormsDataSource);

        public static readonly Guid Language = new(Strings.Language);

        public static readonly Guid IdReservation = new(Strings.IdReservation);

        public static readonly Guid Template = new(Strings.Template);

        public static readonly Guid ContentItem = new(Strings.ContentItem);

        /// <summary>
        ///     Defines the Umbraco object type unique identifiers as string.
        /// </summary>
        /// <remarks>
        ///     Should be used only when it's not possible to use the corresponding
        ///     readonly Guid value, e.g. in attributes (where only consts can be used).
        /// </remarks>
        public static class Strings
        {
            // ReSharper disable MemberHidesStaticFromOuterClass
            public const string DataTypeContainer = "521231E3-8B37-469C-9F9D-51AFC91FEB7B";

            public const string DocumentTypeContainer = "2F7A2769-6B0B-4468-90DD-AF42D64F7F16";

            public const string MediaTypeContainer = "42AEF799-B288-4744-9B10-BE144B73CDC4";

            public const string ContentItem = "10E2B09F-C28B-476D-B77A-AA686435E44A";

            public const string ContentItemType = "7A333C54-6F43-40A4-86A2-18688DC7E532";

            public const string ContentRecycleBin = "01BB7FF2-24DC-4C0C-95A2-C24EF72BBAC8";

            public const string DataType = "30A2A501-1978-4DDB-A57B-F7EFED43BA3C";

            public const string Document = "C66BA18E-EAF3-4CFF-8A22-41B16D66A972";

            public const string DocumentBlueprint = "6EBEF410-03AA-48CF-A792-E1C1CB087ACA";

            public const string DocumentType = "A2CB7800-F571-4787-9638-BC48539A0EFB";

            public const string Media = "B796F64C-1F99-4FFB-B886-4BF4BC011A9C";

            public const string MediaRecycleBin = "CF3D8E34-1C1C-41e9-AE56-878B57B32113";

            public const string MediaType = "4EA4382B-2F5A-4C2B-9587-AE9B3CF3602E";

            public const string Member = "39EB0F98-B348-42A1-8662-E7EB18487560";

            public const string MemberGroup = "366E63B9-880F-4E13-A61C-98069B029728";

            public const string MemberType = "9B5416FB-E72F-45A9-A07B-5A9A2709CE43";

            public const string SystemRoot = "EA7D8624-4CFE-4578-A871-24AA946BF34D";

            public const string Template = "6FBDE604-4178-42CE-A10B-8A2600A2F07D";

            public const string LockObject = "87A9F1FF-B1E4-4A25-BABB-465A4A47EC41";

            public const string RelationType = "B1988FAD-8675-4F47-915A-B3A602BC5D8D";

            public const string FormsForm = "F5A9F787-6593-46F0-B8FF-BFD9BCA9F6BB";

            public const string FormsPreValue = "42D7BF9B-A362-4FEE-B45A-674D5C064B70";

            public const string FormsDataSource = "CFED6CE4-9359-443E-9977-9956FEB1D867";

            public const string Language = "6B05D05B-EC78-49BE-A4E4-79E274F07A77";

            public const string IdReservation = "92849B1E-3904-4713-9356-F646F87C25F4";

            // ReSharper restore MemberHidesStaticFromOuterClass
        }
    }
}
