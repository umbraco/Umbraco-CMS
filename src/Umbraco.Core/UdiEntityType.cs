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
            // note: const fields in this class MUST be consistent with what GetTypes returns
            //  this is validated by UdiTests.ValidateUdiEntityType
            // also, this is used exclusively in Udi static ctor, only once, so there is no
            //  need to keep it around in a field nor to make it readonly

            internal static Dictionary<string, UdiType> GetTypes()
            {
                return new Dictionary<string,UdiType>
                {
                    { Unknown, UdiType.Unknown },

                    { AnyGuid, UdiType.GuidUdi },
                    { Document, UdiType.GuidUdi },
                    { DocumentBluePrint, UdiType.GuidUdi },
                    { Media, UdiType.GuidUdi },
                    { Member, UdiType.GuidUdi },
                    { DictionaryItem, UdiType.GuidUdi },
                    { Macro, UdiType.GuidUdi },
                    { Template, UdiType.GuidUdi },
                    { DocumentType, UdiType.GuidUdi },
                    { DocumentTypeContainer, UdiType.GuidUdi },
                    { DocumentTypeBluePrints, UdiType.GuidUdi },
                    { MediaType, UdiType.GuidUdi },
                    { MediaTypeContainer, UdiType.GuidUdi },
                    { DataType, UdiType.GuidUdi },
                    { DataTypeContainer, UdiType.GuidUdi },
                    { MemberType, UdiType.GuidUdi },
                    { MemberGroup, UdiType.GuidUdi },
                    { RelationType, UdiType.GuidUdi },
                    { FormsForm, UdiType.GuidUdi },
                    { FormsPreValue, UdiType.GuidUdi },
                    { FormsDataSource, UdiType.GuidUdi },

                    { AnyString, UdiType.StringUdi},
                    { Language, UdiType.StringUdi},
                    { MacroScript, UdiType.StringUdi},
                    { MediaFile, UdiType.StringUdi},
                    { TemplateFile, UdiType.StringUdi},
                    { Script, UdiType.StringUdi},
                    { PartialView, UdiType.StringUdi},
                    { PartialViewMacro, UdiType.StringUdi},
                    { Stylesheet, UdiType.StringUdi},
                    { UserControl, UdiType.StringUdi},
                    { Xslt, UdiType.StringUdi},
                };
            }

            public const string Unknown = "unknown";

            // guid entity types

            public const string AnyGuid = "any-guid"; // that one is for tests

            public const string Document = "document";

            public const string DocumentBluePrint = "document-blueprint";

            public const string Media = "media";
            public const string Member = "member";

            public const string DictionaryItem = "dictionary-item";
            public const string Macro = "macro";
            public const string Template = "template";

            public const string DocumentType = "document-type";
            public const string DocumentTypeContainer = "document-type-container";

            public const string DocumentTypeBluePrints = "document-type-blueprints";
            public const string MediaType = "media-type";
            public const string MediaTypeContainer = "media-type-container";
            public const string DataType = "data-type";
            public const string DataTypeContainer = "data-type-container";
            public const string MemberType = "member-type";
            public const string MemberGroup = "member-group";

            public const string RelationType = "relation-type";

            // forms

            public const string FormsForm = "forms-form";
            public const string FormsPreValue = "forms-prevalue";
            public const string FormsDataSource = "forms-datasource";

            // string entity types

            public const string AnyString = "any-string"; // that one is for tests

            public const string Language = "language";
            public const string MacroScript = "macroscript";
            public const string MediaFile = "media-file";
            public const string TemplateFile = "template-file";
            public const string Script = "script";
            public const string Stylesheet = "stylesheet";
            public const string PartialView = "partial-view";
            public const string PartialViewMacro = "partial-view-macro";
            public const string UserControl = "usercontrol";
            public const string Xslt = "xslt";

            public static string FromUmbracoObjectType(UmbracoObjectTypes umbracoObjectType)
            {
                switch (umbracoObjectType)
                {
                    case UmbracoObjectTypes.Document:
                        return Document;
                    case UmbracoObjectTypes.DocumentBlueprint:
                        return DocumentBluePrint;
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
                    case DocumentBluePrint:
                        return UmbracoObjectTypes.DocumentBlueprint;
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
    }
}