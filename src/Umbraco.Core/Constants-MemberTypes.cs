namespace Umbraco.Cms.Core;

public static partial class Constants
{
    public static class MemberTypes
    {
        /// <summary>
        /// Contains GUID constants for built-in member types.
        /// </summary>
        public static class Guids
        {
            /// <summary>
            /// The GUID for the Member member type as a string.
            /// </summary>
            public const string Member = "d59be02f-1df9-4228-aa1e-01917d806cda";

            /// <summary>
            /// The GUID for the Member member type.
            /// </summary>
            public static readonly Guid MemberGuid = new(Member);
        }
    }
}
