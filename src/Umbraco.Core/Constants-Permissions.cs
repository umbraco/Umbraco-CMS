namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Contains the permission verbs and the operations they are built from.
    /// </summary>
    /// <remarks>
    ///     A verb is named <c>{Resource}.{Operation}</c>, like the document permissions (for example
    ///     <c>Umb.Document.Create</c>). <see cref="Umbraco.Cms.Core.Permissions.PermissionVerbs.For"/> builds a verb
    ///     the same way.
    /// </remarks>
    public static class Permissions
    {
        /// <summary>
        ///     Contains the operations that Umbraco's permission verbs are built from. An operation is the last part
        ///     of a verb.
        /// </summary>
        /// <remarks>
        ///     These are the operations the Management API infers from an endpoint's HTTP method. A package or product
        ///     can build verbs for other operations with
        ///     <see cref="Umbraco.Cms.Core.Permissions.PermissionVerbs.For"/>.
        /// </remarks>
        public static class Operations
        {
            /// <summary>
            ///     Creating a new entity.
            /// </summary>
            public const string Create = "Create";

            /// <summary>
            ///     Modifying an existing entity.
            /// </summary>
            public const string Update = "Update";

            /// <summary>
            ///     Removing an existing entity.
            /// </summary>
            public const string Delete = "Delete";
        }

        /// <summary>
        ///     Contains the permission verbs for operations on entity types, each named
        ///     <c>Umb.{EntityType}.{Operation}</c>.
        /// </summary>
        public static class Entities
        {
            /// <summary>
            ///     Contains the permission verbs for data types.
            /// </summary>
            public static class DataType
            {
                /// <summary>
                ///     The resource this entity type's permission verbs are named after.
                /// </summary>
                public const string Resource = "Umb.DataType";

                /// <summary>
                ///     Creating a data type.
                /// </summary>
                public const string Create = Resource + "." + Operations.Create;

                /// <summary>
                ///     Updating a data type.
                /// </summary>
                public const string Update = Resource + "." + Operations.Update;

                /// <summary>
                ///     Deleting a data type.
                /// </summary>
                public const string Delete = Resource + "." + Operations.Delete;
            }

            /// <summary>
            ///     Contains the permission verbs for dictionary items.
            /// </summary>
            public static class DictionaryItem
            {
                /// <summary>
                ///     The resource this entity type's permission verbs are named after.
                /// </summary>
                public const string Resource = "Umb.DictionaryItem";

                /// <summary>
                ///     Creating a dictionary item.
                /// </summary>
                public const string Create = Resource + "." + Operations.Create;

                /// <summary>
                ///     Updating a dictionary item.
                /// </summary>
                public const string Update = Resource + "." + Operations.Update;

                /// <summary>
                ///     Deleting a dictionary item.
                /// </summary>
                public const string Delete = Resource + "." + Operations.Delete;
            }

            /// <summary>
            ///     Contains the permission verbs for document types.
            /// </summary>
            public static class DocumentType
            {
                /// <summary>
                ///     The resource this entity type's permission verbs are named after.
                /// </summary>
                public const string Resource = "Umb.DocumentType";

                /// <summary>
                ///     Creating a document type.
                /// </summary>
                public const string Create = Resource + "." + Operations.Create;

                /// <summary>
                ///     Updating a document type.
                /// </summary>
                public const string Update = Resource + "." + Operations.Update;

                /// <summary>
                ///     Deleting a document type.
                /// </summary>
                public const string Delete = Resource + "." + Operations.Delete;
            }

            /// <summary>
            ///     Contains the permission verbs for languages.
            /// </summary>
            public static class Language
            {
                /// <summary>
                ///     The resource this entity type's permission verbs are named after.
                /// </summary>
                public const string Resource = "Umb.Language";

                /// <summary>
                ///     Creating a language.
                /// </summary>
                public const string Create = Resource + "." + Operations.Create;

                /// <summary>
                ///     Updating a language.
                /// </summary>
                public const string Update = Resource + "." + Operations.Update;

                /// <summary>
                ///     Deleting a language.
                /// </summary>
                public const string Delete = Resource + "." + Operations.Delete;
            }

            /// <summary>
            ///     Contains the permission verbs for media types.
            /// </summary>
            public static class MediaType
            {
                /// <summary>
                ///     The resource this entity type's permission verbs are named after.
                /// </summary>
                public const string Resource = "Umb.MediaType";

                /// <summary>
                ///     Creating a media type.
                /// </summary>
                public const string Create = Resource + "." + Operations.Create;

                /// <summary>
                ///     Updating a media type.
                /// </summary>
                public const string Update = Resource + "." + Operations.Update;

                /// <summary>
                ///     Deleting a media type.
                /// </summary>
                public const string Delete = Resource + "." + Operations.Delete;
            }

            /// <summary>
            ///     Contains the permission verbs for member groups.
            /// </summary>
            public static class MemberGroup
            {
                /// <summary>
                ///     The resource this entity type's permission verbs are named after.
                /// </summary>
                public const string Resource = "Umb.MemberGroup";

                /// <summary>
                ///     Creating a member group.
                /// </summary>
                public const string Create = Resource + "." + Operations.Create;

                /// <summary>
                ///     Updating a member group.
                /// </summary>
                public const string Update = Resource + "." + Operations.Update;

                /// <summary>
                ///     Deleting a member group.
                /// </summary>
                public const string Delete = Resource + "." + Operations.Delete;
            }

            /// <summary>
            ///     Contains the permission verbs for member types.
            /// </summary>
            public static class MemberType
            {
                /// <summary>
                ///     The resource this entity type's permission verbs are named after.
                /// </summary>
                public const string Resource = "Umb.MemberType";

                /// <summary>
                ///     Creating a member type.
                /// </summary>
                public const string Create = Resource + "." + Operations.Create;

                /// <summary>
                ///     Updating a member type.
                /// </summary>
                public const string Update = Resource + "." + Operations.Update;

                /// <summary>
                ///     Deleting a member type.
                /// </summary>
                public const string Delete = Resource + "." + Operations.Delete;
            }

            /// <summary>
            ///     Contains the permission verbs for partial views.
            /// </summary>
            public static class PartialView
            {
                /// <summary>
                ///     The resource this entity type's permission verbs are named after.
                /// </summary>
                public const string Resource = "Umb.PartialView";

                /// <summary>
                ///     Creating a partial view.
                /// </summary>
                public const string Create = Resource + "." + Operations.Create;

                /// <summary>
                ///     Updating a partial view.
                /// </summary>
                public const string Update = Resource + "." + Operations.Update;

                /// <summary>
                ///     Deleting a partial view.
                /// </summary>
                public const string Delete = Resource + "." + Operations.Delete;
            }

            /// <summary>
            ///     Contains the permission verbs for scripts.
            /// </summary>
            public static class Script
            {
                /// <summary>
                ///     The resource this entity type's permission verbs are named after.
                /// </summary>
                public const string Resource = "Umb.Script";

                /// <summary>
                ///     Creating a script.
                /// </summary>
                public const string Create = Resource + "." + Operations.Create;

                /// <summary>
                ///     Updating a script.
                /// </summary>
                public const string Update = Resource + "." + Operations.Update;

                /// <summary>
                ///     Deleting a script.
                /// </summary>
                public const string Delete = Resource + "." + Operations.Delete;
            }

            /// <summary>
            ///     Contains the permission verbs for stylesheets.
            /// </summary>
            public static class Stylesheet
            {
                /// <summary>
                ///     The resource this entity type's permission verbs are named after.
                /// </summary>
                public const string Resource = "Umb.Stylesheet";

                /// <summary>
                ///     Creating a stylesheet.
                /// </summary>
                public const string Create = Resource + "." + Operations.Create;

                /// <summary>
                ///     Updating a stylesheet.
                /// </summary>
                public const string Update = Resource + "." + Operations.Update;

                /// <summary>
                ///     Deleting a stylesheet.
                /// </summary>
                public const string Delete = Resource + "." + Operations.Delete;
            }

            /// <summary>
            ///     Contains the permission verbs for templates.
            /// </summary>
            public static class Template
            {
                /// <summary>
                ///     The resource this entity type's permission verbs are named after.
                /// </summary>
                public const string Resource = "Umb.Template";

                /// <summary>
                ///     Creating a template.
                /// </summary>
                public const string Create = Resource + "." + Operations.Create;

                /// <summary>
                ///     Updating a template.
                /// </summary>
                public const string Update = Resource + "." + Operations.Update;

                /// <summary>
                ///     Deleting a template.
                /// </summary>
                public const string Delete = Resource + "." + Operations.Delete;
            }

            /// <summary>
            ///     Contains the permission verbs for webhooks.
            /// </summary>
            public static class Webhook
            {
                /// <summary>
                ///     The resource this entity type's permission verbs are named after.
                /// </summary>
                public const string Resource = "Umb.Webhook";

                /// <summary>
                ///     Creating a webhook.
                /// </summary>
                public const string Create = Resource + "." + Operations.Create;

                /// <summary>
                ///     Updating a webhook.
                /// </summary>
                public const string Update = Resource + "." + Operations.Update;

                /// <summary>
                ///     Deleting a webhook.
                /// </summary>
                public const string Delete = Resource + "." + Operations.Delete;
            }
        }
    }
}
