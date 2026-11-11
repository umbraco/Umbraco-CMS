namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Contains the permission verbs for operations on entity types.
    /// </summary>
    /// <remarks>
    ///     A verb is named <c>{Prefix}.{Operation}</c>, where the prefix is <c>Umb.{EntityType}</c>, like the document
    ///     permissions (for example <c>Umb.Document.Create</c>).
    /// </remarks>
    public static class Permissions
    {
        /// <summary>
        ///     Contains the permission verbs for data types.
        /// </summary>
        public static class DataType
        {
            /// <summary>
            ///     The prefix of the permission verbs for this entity type.
            /// </summary>
            public const string Prefix = "Umb.DataType";

            /// <summary>
            ///     Creating a data type.
            /// </summary>
            public const string Create = Prefix + ".Create";

            /// <summary>
            ///     Updating a data type.
            /// </summary>
            public const string Update = Prefix + ".Update";

            /// <summary>
            ///     Deleting a data type.
            /// </summary>
            public const string Delete = Prefix + ".Delete";
        }

        /// <summary>
        ///     Contains the permission verbs for dictionary items.
        /// </summary>
        public static class DictionaryItem
        {
            /// <summary>
            ///     The prefix of the permission verbs for this entity type.
            /// </summary>
            public const string Prefix = "Umb.DictionaryItem";

            /// <summary>
            ///     Creating a dictionary item.
            /// </summary>
            public const string Create = Prefix + ".Create";

            /// <summary>
            ///     Updating a dictionary item.
            /// </summary>
            public const string Update = Prefix + ".Update";

            /// <summary>
            ///     Deleting a dictionary item.
            /// </summary>
            public const string Delete = Prefix + ".Delete";
        }

        /// <summary>
        ///     Contains the permission verbs for document types.
        /// </summary>
        public static class DocumentType
        {
            /// <summary>
            ///     The prefix of the permission verbs for this entity type.
            /// </summary>
            public const string Prefix = "Umb.DocumentType";

            /// <summary>
            ///     Creating a document type.
            /// </summary>
            public const string Create = Prefix + ".Create";

            /// <summary>
            ///     Updating a document type.
            /// </summary>
            public const string Update = Prefix + ".Update";

            /// <summary>
            ///     Deleting a document type.
            /// </summary>
            public const string Delete = Prefix + ".Delete";
        }

        /// <summary>
        ///     Contains the permission verbs for languages.
        /// </summary>
        public static class Language
        {
            /// <summary>
            ///     The prefix of the permission verbs for this entity type.
            /// </summary>
            public const string Prefix = "Umb.Language";

            /// <summary>
            ///     Creating a language.
            /// </summary>
            public const string Create = Prefix + ".Create";

            /// <summary>
            ///     Updating a language.
            /// </summary>
            public const string Update = Prefix + ".Update";

            /// <summary>
            ///     Deleting a language.
            /// </summary>
            public const string Delete = Prefix + ".Delete";
        }

        /// <summary>
        ///     Contains the permission verbs for media types.
        /// </summary>
        public static class MediaType
        {
            /// <summary>
            ///     The prefix of the permission verbs for this entity type.
            /// </summary>
            public const string Prefix = "Umb.MediaType";

            /// <summary>
            ///     Creating a media type.
            /// </summary>
            public const string Create = Prefix + ".Create";

            /// <summary>
            ///     Updating a media type.
            /// </summary>
            public const string Update = Prefix + ".Update";

            /// <summary>
            ///     Deleting a media type.
            /// </summary>
            public const string Delete = Prefix + ".Delete";
        }

        /// <summary>
        ///     Contains the permission verbs for member groups.
        /// </summary>
        public static class MemberGroup
        {
            /// <summary>
            ///     The prefix of the permission verbs for this entity type.
            /// </summary>
            public const string Prefix = "Umb.MemberGroup";

            /// <summary>
            ///     Creating a member group.
            /// </summary>
            public const string Create = Prefix + ".Create";

            /// <summary>
            ///     Updating a member group.
            /// </summary>
            public const string Update = Prefix + ".Update";

            /// <summary>
            ///     Deleting a member group.
            /// </summary>
            public const string Delete = Prefix + ".Delete";
        }

        /// <summary>
        ///     Contains the permission verbs for member types.
        /// </summary>
        public static class MemberType
        {
            /// <summary>
            ///     The prefix of the permission verbs for this entity type.
            /// </summary>
            public const string Prefix = "Umb.MemberType";

            /// <summary>
            ///     Creating a member type.
            /// </summary>
            public const string Create = Prefix + ".Create";

            /// <summary>
            ///     Updating a member type.
            /// </summary>
            public const string Update = Prefix + ".Update";

            /// <summary>
            ///     Deleting a member type.
            /// </summary>
            public const string Delete = Prefix + ".Delete";
        }

        /// <summary>
        ///     Contains the permission verbs for partial views.
        /// </summary>
        public static class PartialView
        {
            /// <summary>
            ///     The prefix of the permission verbs for this entity type.
            /// </summary>
            public const string Prefix = "Umb.PartialView";

            /// <summary>
            ///     Creating a partial view.
            /// </summary>
            public const string Create = Prefix + ".Create";

            /// <summary>
            ///     Updating a partial view.
            /// </summary>
            public const string Update = Prefix + ".Update";

            /// <summary>
            ///     Deleting a partial view.
            /// </summary>
            public const string Delete = Prefix + ".Delete";
        }

        /// <summary>
        ///     Contains the permission verbs for scripts.
        /// </summary>
        public static class Script
        {
            /// <summary>
            ///     The prefix of the permission verbs for this entity type.
            /// </summary>
            public const string Prefix = "Umb.Script";

            /// <summary>
            ///     Creating a script.
            /// </summary>
            public const string Create = Prefix + ".Create";

            /// <summary>
            ///     Updating a script.
            /// </summary>
            public const string Update = Prefix + ".Update";

            /// <summary>
            ///     Deleting a script.
            /// </summary>
            public const string Delete = Prefix + ".Delete";
        }

        /// <summary>
        ///     Contains the permission verbs for stylesheets.
        /// </summary>
        public static class Stylesheet
        {
            /// <summary>
            ///     The prefix of the permission verbs for this entity type.
            /// </summary>
            public const string Prefix = "Umb.Stylesheet";

            /// <summary>
            ///     Creating a stylesheet.
            /// </summary>
            public const string Create = Prefix + ".Create";

            /// <summary>
            ///     Updating a stylesheet.
            /// </summary>
            public const string Update = Prefix + ".Update";

            /// <summary>
            ///     Deleting a stylesheet.
            /// </summary>
            public const string Delete = Prefix + ".Delete";
        }

        /// <summary>
        ///     Contains the permission verbs for templates.
        /// </summary>
        public static class Template
        {
            /// <summary>
            ///     The prefix of the permission verbs for this entity type.
            /// </summary>
            public const string Prefix = "Umb.Template";

            /// <summary>
            ///     Creating a template.
            /// </summary>
            public const string Create = Prefix + ".Create";

            /// <summary>
            ///     Updating a template.
            /// </summary>
            public const string Update = Prefix + ".Update";

            /// <summary>
            ///     Deleting a template.
            /// </summary>
            public const string Delete = Prefix + ".Delete";
        }

        /// <summary>
        ///     Contains the permission verbs for webhooks.
        /// </summary>
        public static class Webhook
        {
            /// <summary>
            ///     The prefix of the permission verbs for this entity type.
            /// </summary>
            public const string Prefix = "Umb.Webhook";

            /// <summary>
            ///     Creating a webhook.
            /// </summary>
            public const string Create = Prefix + ".Create";

            /// <summary>
            ///     Updating a webhook.
            /// </summary>
            public const string Update = Prefix + ".Update";

            /// <summary>
            ///     Deleting a webhook.
            /// </summary>
            public const string Delete = Prefix + ".Delete";
        }
    }
}
