namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Contains the permission verbs for operations on entity types.
    /// </summary>
    /// <remarks>
    ///     A verb is named <c>Umb.{EntityType}.{Operation}</c>, like the document permissions (for example
    ///     <c>Umb.Document.Create</c>).
    /// </remarks>
    public static class Permissions
    {
        /// <summary>
        ///     Contains the permission verbs for data types.
        /// </summary>
        public static class DataType
        {
            /// <summary>
            ///     Creating a data type.
            /// </summary>
            public const string Create = "Umb.DataType.Create";

            /// <summary>
            ///     Updating a data type.
            /// </summary>
            public const string Update = "Umb.DataType.Update";

            /// <summary>
            ///     Deleting a data type.
            /// </summary>
            public const string Delete = "Umb.DataType.Delete";
        }

        /// <summary>
        ///     Contains the permission verbs for dictionary items.
        /// </summary>
        public static class DictionaryItem
        {
            /// <summary>
            ///     Creating a dictionary item.
            /// </summary>
            public const string Create = "Umb.DictionaryItem.Create";

            /// <summary>
            ///     Updating a dictionary item.
            /// </summary>
            public const string Update = "Umb.DictionaryItem.Update";

            /// <summary>
            ///     Deleting a dictionary item.
            /// </summary>
            public const string Delete = "Umb.DictionaryItem.Delete";
        }

        /// <summary>
        ///     Contains the permission verbs for document types.
        /// </summary>
        public static class DocumentType
        {
            /// <summary>
            ///     Creating a document type.
            /// </summary>
            public const string Create = "Umb.DocumentType.Create";

            /// <summary>
            ///     Updating a document type.
            /// </summary>
            public const string Update = "Umb.DocumentType.Update";

            /// <summary>
            ///     Deleting a document type.
            /// </summary>
            public const string Delete = "Umb.DocumentType.Delete";
        }

        /// <summary>
        ///     Contains the permission verbs for languages.
        /// </summary>
        public static class Language
        {
            /// <summary>
            ///     Creating a language.
            /// </summary>
            public const string Create = "Umb.Language.Create";

            /// <summary>
            ///     Updating a language.
            /// </summary>
            public const string Update = "Umb.Language.Update";

            /// <summary>
            ///     Deleting a language.
            /// </summary>
            public const string Delete = "Umb.Language.Delete";
        }

        /// <summary>
        ///     Contains the permission verbs for media types.
        /// </summary>
        public static class MediaType
        {
            /// <summary>
            ///     Creating a media type.
            /// </summary>
            public const string Create = "Umb.MediaType.Create";

            /// <summary>
            ///     Updating a media type.
            /// </summary>
            public const string Update = "Umb.MediaType.Update";

            /// <summary>
            ///     Deleting a media type.
            /// </summary>
            public const string Delete = "Umb.MediaType.Delete";
        }

        /// <summary>
        ///     Contains the permission verbs for member groups.
        /// </summary>
        public static class MemberGroup
        {
            /// <summary>
            ///     Creating a member group.
            /// </summary>
            public const string Create = "Umb.MemberGroup.Create";

            /// <summary>
            ///     Updating a member group.
            /// </summary>
            public const string Update = "Umb.MemberGroup.Update";

            /// <summary>
            ///     Deleting a member group.
            /// </summary>
            public const string Delete = "Umb.MemberGroup.Delete";
        }

        /// <summary>
        ///     Contains the permission verbs for member types.
        /// </summary>
        public static class MemberType
        {
            /// <summary>
            ///     Creating a member type.
            /// </summary>
            public const string Create = "Umb.MemberType.Create";

            /// <summary>
            ///     Updating a member type.
            /// </summary>
            public const string Update = "Umb.MemberType.Update";

            /// <summary>
            ///     Deleting a member type.
            /// </summary>
            public const string Delete = "Umb.MemberType.Delete";
        }

        /// <summary>
        ///     Contains the permission verbs for partial views.
        /// </summary>
        public static class PartialView
        {
            /// <summary>
            ///     Creating a partial view.
            /// </summary>
            public const string Create = "Umb.PartialView.Create";

            /// <summary>
            ///     Updating a partial view.
            /// </summary>
            public const string Update = "Umb.PartialView.Update";

            /// <summary>
            ///     Deleting a partial view.
            /// </summary>
            public const string Delete = "Umb.PartialView.Delete";
        }

        /// <summary>
        ///     Contains the permission verbs for scripts.
        /// </summary>
        public static class Script
        {
            /// <summary>
            ///     Creating a script.
            /// </summary>
            public const string Create = "Umb.Script.Create";

            /// <summary>
            ///     Updating a script.
            /// </summary>
            public const string Update = "Umb.Script.Update";

            /// <summary>
            ///     Deleting a script.
            /// </summary>
            public const string Delete = "Umb.Script.Delete";
        }

        /// <summary>
        ///     Contains the permission verbs for stylesheets.
        /// </summary>
        public static class Stylesheet
        {
            /// <summary>
            ///     Creating a stylesheet.
            /// </summary>
            public const string Create = "Umb.Stylesheet.Create";

            /// <summary>
            ///     Updating a stylesheet.
            /// </summary>
            public const string Update = "Umb.Stylesheet.Update";

            /// <summary>
            ///     Deleting a stylesheet.
            /// </summary>
            public const string Delete = "Umb.Stylesheet.Delete";
        }

        /// <summary>
        ///     Contains the permission verbs for templates.
        /// </summary>
        public static class Template
        {
            /// <summary>
            ///     Creating a template.
            /// </summary>
            public const string Create = "Umb.Template.Create";

            /// <summary>
            ///     Updating a template.
            /// </summary>
            public const string Update = "Umb.Template.Update";

            /// <summary>
            ///     Deleting a template.
            /// </summary>
            public const string Delete = "Umb.Template.Delete";
        }

        /// <summary>
        ///     Contains the permission verbs for webhooks.
        /// </summary>
        public static class Webhook
        {
            /// <summary>
            ///     Creating a webhook.
            /// </summary>
            public const string Create = "Umb.Webhook.Create";

            /// <summary>
            ///     Updating a webhook.
            /// </summary>
            public const string Update = "Umb.Webhook.Update";

            /// <summary>
            ///     Deleting a webhook.
            /// </summary>
            public const string Delete = "Umb.Webhook.Delete";
        }
    }
}
