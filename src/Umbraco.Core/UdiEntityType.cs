using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core
{
    
    public static partial class Constants
    {

        /// <summary>
        /// Defines well-known entity types.
        /// </summary>
        /// <remarks>Well-known entity types are those that Deploy already knows about,
        /// but entity types are strings and so can be extended beyond what is defined here.</remarks>
        public static class UdiEntityType
        {
            public const string Unknown = "unknown";

            // guid entity types

            public const string AnyGuid = "any-guid"; // that one is for tests
            

            public const string Document = "document";
            public const string Media = "media";
            public const string Member = "member";

            public const string DictionaryItem = "dictionary-item";
            public const string Macro = "macro";
            public const string Template = "template";

            public const string DocumentType = "document-type";
            public const string DocumentTypeContainer = "document-type-container";
            public const string MediaType = "media-type";
            public const string MediaTypeContainer = "media-type-container";
            public const string DataType = "data-type";
            public const string DataTypeContainer = "data-type-container";
            public const string MemberType = "member-type";
            public const string MemberGroup = "member-group";

            public const string RelationType = "relation-type";

            // string entity types

            public const string AnyString = "any-string"; // that one is for tests

            public const string MediaFile = "media-file";
            public const string TemplateFile = "template-file";
            public const string Script = "script";
            public const string Stylesheet = "stylesheet";
            public const string PartialView = "partial-view";
            public const string PartialViewMacro = "partial-view-macro";
            public const string Xslt = "xslt";

            public static string FromUmbracoObjectType(UmbracoObjectTypes umbracoObjectType)
            {
                switch (umbracoObjectType)
                {
                    case UmbracoObjectTypes.Document:
                        return Document;
                    case UmbracoObjectTypes.Media:
                        return Media;
                    case UmbracoObjectTypes.Member:
                        return Member;
                    case UmbracoObjectTypes.Template:
                        return Template;
                    case UmbracoObjectTypes.DocumentType:
                        return DocumentType;
                    case UmbracoObjectTypes.DocumentTypeContainer:
                        return DocumentTypeContainer;
                    case UmbracoObjectTypes.MediaType:
                        return MediaType;
                    case UmbracoObjectTypes.MediaTypeContainer:
                        return MediaTypeContainer;
                    case UmbracoObjectTypes.DataType:
                        return DataType;
                    case UmbracoObjectTypes.DataTypeContainer:
                        return DataTypeContainer;
                    case UmbracoObjectTypes.MemberType:
                        return MemberType;
                    case UmbracoObjectTypes.MemberGroup:
                        return MemberGroup;
                    case UmbracoObjectTypes.Stylesheet:
                        return Stylesheet;
                    case UmbracoObjectTypes.RelationType:
                        return RelationType;
                }
                throw new NotSupportedException(string.Format("UmbracoObjectType \"{0}\" does not have a matching EntityType.", umbracoObjectType));
            }

            public static UmbracoObjectTypes ToUmbracoObjectType(string entityType)
            {
                switch (entityType)
                {
                    case Document:
                        return UmbracoObjectTypes.Document;
                    case Media:
                        return UmbracoObjectTypes.Media;
                    case Member:
                        return UmbracoObjectTypes.Member;
                    case Template:
                        return UmbracoObjectTypes.Template;
                    case DocumentType:
                        return UmbracoObjectTypes.DocumentType;
                    case DocumentTypeContainer:
                        return UmbracoObjectTypes.DocumentTypeContainer;
                    case MediaType:
                        return UmbracoObjectTypes.MediaType;
                    case MediaTypeContainer:
                        return UmbracoObjectTypes.MediaTypeContainer;
                    case DataType:
                        return UmbracoObjectTypes.DataType;
                    case DataTypeContainer:
                        return UmbracoObjectTypes.DataTypeContainer;
                    case MemberType:
                        return UmbracoObjectTypes.MemberType;
                    case MemberGroup:
                        return UmbracoObjectTypes.MemberGroup;
                    case Stylesheet:
                        return UmbracoObjectTypes.Stylesheet;
                    case RelationType:
                        return UmbracoObjectTypes.RelationType;
                }
                throw new NotSupportedException(
                    string.Format("EntityType \"{0}\" does not have a matching UmbracoObjectType.", entityType));
            }
        }


    }
}