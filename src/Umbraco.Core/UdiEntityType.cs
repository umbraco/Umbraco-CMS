using System;
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
            [UdiType(UdiType.Unknown)]
            public const string Unknown = "unknown";

            // guid entity types

            [UdiType(UdiType.GuidUdi)]
            public const string AnyGuid = "any-guid"; // that one is for tests

            [UdiType(UdiType.GuidUdi)]
            public const string Document = "document";
            [UdiType(UdiType.GuidUdi)]
            public const string Media = "media";
            [UdiType(UdiType.GuidUdi)]
            public const string Member = "member";

            [UdiType(UdiType.GuidUdi)]
            public const string DictionaryItem = "dictionary-item";
            [UdiType(UdiType.GuidUdi)]
            public const string Macro = "macro";
            [UdiType(UdiType.GuidUdi)]
            public const string Template = "template";

            [UdiType(UdiType.GuidUdi)]
            public const string DocumentType = "document-type";
            [UdiType(UdiType.GuidUdi)]
            public const string DocumentTypeContainer = "document-type-container";
            [UdiType(UdiType.GuidUdi)]
            public const string MediaType = "media-type";
            [UdiType(UdiType.GuidUdi)]
            public const string MediaTypeContainer = "media-type-container";
            [UdiType(UdiType.GuidUdi)]
            public const string DataType = "data-type";
            [UdiType(UdiType.GuidUdi)]
            public const string DataTypeContainer = "data-type-container";
            [UdiType(UdiType.GuidUdi)]
            public const string MemberType = "member-type";
            [UdiType(UdiType.GuidUdi)]
            public const string MemberGroup = "member-group";

            [UdiType(UdiType.GuidUdi)]
            public const string RelationType = "relation-type";

            // forms

            [UdiType(UdiType.GuidUdi)]
            public const string FormsForm = "forms-form";
            [UdiType(UdiType.GuidUdi)]
            public const string FormsPreValue = "forms-prevalue";
            [UdiType(UdiType.GuidUdi)]
            public const string FormsDataSource = "forms-datasource";

            // string entity types

            [UdiType(UdiType.StringUdi)]
            public const string AnyString = "any-string"; // that one is for tests

            [UdiType(UdiType.StringUdi)]
            public const string Language = "language";
            [UdiType(UdiType.StringUdi)]
            public const string MacroScript = "macroscript";
            [UdiType(UdiType.StringUdi)]
            public const string MediaFile = "media-file";
            [UdiType(UdiType.StringUdi)]
            public const string TemplateFile = "template-file";
            [UdiType(UdiType.StringUdi)]
            public const string Script = "script";
            [UdiType(UdiType.StringUdi)]
            public const string Stylesheet = "stylesheet";
            [UdiType(UdiType.StringUdi)]
            public const string PartialView = "partial-view";
            [UdiType(UdiType.StringUdi)]
            public const string PartialViewMacro = "partial-view-macro";
            [UdiType(UdiType.StringUdi)]
            public const string UserControl = "usercontrol";
            [UdiType(UdiType.StringUdi)]
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
                    case UmbracoObjectTypes.FormsForm:
                        return FormsForm;
                    case UmbracoObjectTypes.FormsPreValue:
                        return FormsPreValue;
                    case UmbracoObjectTypes.FormsDataSource:
                        return FormsDataSource;
                    case UmbracoObjectTypes.Language:
                        return Language;
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
                    case FormsForm:
                        return UmbracoObjectTypes.FormsForm;
                    case FormsPreValue:
                        return UmbracoObjectTypes.FormsPreValue;
                    case FormsDataSource:
                        return UmbracoObjectTypes.FormsDataSource;
                    case Language:
                        return UmbracoObjectTypes.Language;
                }
                throw new NotSupportedException(
                    string.Format("EntityType \"{0}\" does not have a matching UmbracoObjectType.", entityType));
            }
        }

        [AttributeUsage(AttributeTargets.Field)]
        internal class UdiTypeAttribute : Attribute
        {
            public UdiType UdiType { get; private set; }

            public UdiTypeAttribute(UdiType udiType)
            {
                UdiType = udiType;
            }
        }
    }
}