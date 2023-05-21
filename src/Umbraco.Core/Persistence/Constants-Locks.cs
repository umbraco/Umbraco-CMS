// ReSharper disable once CheckNamespace

using Umbraco.Cms.Core.Runtime;

namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Defines lock objects.
    /// </summary>
    public static class Locks
    {
        /// <summary>
        ///     The <see cref="IMainDom" /> lock
        /// </summary>
        public const int MainDom = -1000;

        /// <summary>
        ///     All servers.
        /// </summary>
        public const int Servers = -331;

        /// <summary>
        ///     All content and media types.
        /// </summary>
        public const int ContentTypes = -332;

        /// <summary>
        ///     The entire content tree, i.e. all content items.
        /// </summary>
        public const int ContentTree = -333;

        /// <summary>
        ///     The entire media tree, i.e. all media items.
        /// </summary>
        public const int MediaTree = -334;

        /// <summary>
        ///     The entire member tree, i.e. all members.
        /// </summary>
        public const int MemberTree = -335;

        /// <summary>
        ///     All media types.
        /// </summary>
        public const int MediaTypes = -336;

        /// <summary>
        ///     All member types.
        /// </summary>
        public const int MemberTypes = -337;

        /// <summary>
        ///     All domains.
        /// </summary>
        public const int Domains = -338;

        /// <summary>
        ///     All key-values.
        /// </summary>
        public const int KeyValues = -339;

        /// <summary>
        ///     All languages.
        /// </summary>
        public const int Languages = -340;

        /// <summary>
        ///     ScheduledPublishing job.
        /// </summary>
        public const int ScheduledPublishing = -341;
    }
}
