using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core
{
    public static class UdiEntityTypeHelper
    {
        internal static Dictionary<string, UdiType> GetTypes() =>
            new Dictionary<string, UdiType>
            {
                { Constants.UdiEntityType.Unknown, UdiType.Unknown },

                { Constants.UdiEntityType.AnyGuid, UdiType.GuidUdi },
                { Constants.UdiEntityType.Document, UdiType.GuidUdi },
                { Constants.UdiEntityType.DocumentBlueprint, UdiType.GuidUdi },
                { Constants.UdiEntityType.Media, UdiType.GuidUdi },
                { Constants.UdiEntityType.Member, UdiType.GuidUdi },
                { Constants.UdiEntityType.DictionaryItem, UdiType.GuidUdi },
                { Constants.UdiEntityType.Macro, UdiType.GuidUdi },
                { Constants.UdiEntityType.Template, UdiType.GuidUdi },
                { Constants.UdiEntityType.DocumentType, UdiType.GuidUdi },
                { Constants.UdiEntityType.DocumentTypeContainer, UdiType.GuidUdi },
                { Constants.UdiEntityType.DocumentTypeBluePrints, UdiType.GuidUdi },
                { Constants.UdiEntityType.MediaType, UdiType.GuidUdi },
                { Constants.UdiEntityType.MediaTypeContainer, UdiType.GuidUdi },
                { Constants.UdiEntityType.DataType, UdiType.GuidUdi },
                { Constants.UdiEntityType.DataTypeContainer, UdiType.GuidUdi },
                { Constants.UdiEntityType.MemberType, UdiType.GuidUdi },
                { Constants.UdiEntityType.MemberGroup, UdiType.GuidUdi },
                { Constants.UdiEntityType.RelationType, UdiType.GuidUdi },
                { Constants.UdiEntityType.FormsForm, UdiType.GuidUdi },
                { Constants.UdiEntityType.FormsPreValue, UdiType.GuidUdi },
                { Constants.UdiEntityType.FormsDataSource, UdiType.GuidUdi },

                { Constants.UdiEntityType.AnyString, UdiType.StringUdi },
                { Constants.UdiEntityType.Language, UdiType.StringUdi },
                { Constants.UdiEntityType.MacroScript, UdiType.StringUdi },
                { Constants.UdiEntityType.MediaFile, UdiType.StringUdi },
                { Constants.UdiEntityType.TemplateFile, UdiType.StringUdi },
                { Constants.UdiEntityType.Script, UdiType.StringUdi },
                { Constants.UdiEntityType.PartialView, UdiType.StringUdi },
                { Constants.UdiEntityType.PartialViewMacro, UdiType.StringUdi },
                { Constants.UdiEntityType.Stylesheet, UdiType.StringUdi }
            };

        public static string FromUmbracoObjectType(UmbracoObjectTypes umbracoObjectType)
        {
            switch (umbracoObjectType)
            {
                case UmbracoObjectTypes.Document:
                    return Constants.UdiEntityType.Document;
                case UmbracoObjectTypes.DocumentBlueprint:
                    return Constants.UdiEntityType.DocumentBlueprint;
                case UmbracoObjectTypes.Media:
                    return Constants.UdiEntityType.Media;
                case UmbracoObjectTypes.Member:
                    return Constants.UdiEntityType.Member;
                case UmbracoObjectTypes.Template:
                    return Constants.UdiEntityType.Template;
                case UmbracoObjectTypes.DocumentType:
                    return Constants.UdiEntityType.DocumentType;
                case UmbracoObjectTypes.DocumentTypeContainer:
                    return Constants.UdiEntityType.DocumentTypeContainer;
                case UmbracoObjectTypes.MediaType:
                    return Constants.UdiEntityType.MediaType;
                case UmbracoObjectTypes.MediaTypeContainer:
                    return Constants.UdiEntityType.MediaTypeContainer;
                case UmbracoObjectTypes.DataType:
                    return Constants.UdiEntityType.DataType;
                case UmbracoObjectTypes.DataTypeContainer:
                    return Constants.UdiEntityType.DataTypeContainer;
                case UmbracoObjectTypes.MemberType:
                    return Constants.UdiEntityType.MemberType;
                case UmbracoObjectTypes.MemberGroup:
                    return Constants.UdiEntityType.MemberGroup;
                case UmbracoObjectTypes.Stylesheet:
                    return Constants.UdiEntityType.Stylesheet;
                case UmbracoObjectTypes.RelationType:
                    return Constants.UdiEntityType.RelationType;
                case UmbracoObjectTypes.FormsForm:
                    return Constants.UdiEntityType.FormsForm;
                case UmbracoObjectTypes.FormsPreValue:
                    return Constants.UdiEntityType.FormsPreValue;
                case UmbracoObjectTypes.FormsDataSource:
                    return Constants.UdiEntityType.FormsDataSource;
                case UmbracoObjectTypes.Language:
                    return Constants.UdiEntityType.Language;
            }

            throw new NotSupportedException(
                $"UmbracoObjectType \"{umbracoObjectType}\" does not have a matching EntityType.");
        }

        public static UmbracoObjectTypes ToUmbracoObjectType(string entityType)
        {
            switch (entityType)
            {
                case Constants.UdiEntityType.Document:
                    return UmbracoObjectTypes.Document;
                case Constants.UdiEntityType.DocumentBlueprint:
                    return UmbracoObjectTypes.DocumentBlueprint;
                case Constants.UdiEntityType.Media:
                    return UmbracoObjectTypes.Media;
                case Constants.UdiEntityType.Member:
                    return UmbracoObjectTypes.Member;
                case Constants.UdiEntityType.Template:
                    return UmbracoObjectTypes.Template;
                case Constants.UdiEntityType.DocumentType:
                    return UmbracoObjectTypes.DocumentType;
                case Constants.UdiEntityType.DocumentTypeContainer:
                    return UmbracoObjectTypes.DocumentTypeContainer;
                case Constants.UdiEntityType.MediaType:
                    return UmbracoObjectTypes.MediaType;
                case Constants.UdiEntityType.MediaTypeContainer:
                    return UmbracoObjectTypes.MediaTypeContainer;
                case Constants.UdiEntityType.DataType:
                    return UmbracoObjectTypes.DataType;
                case Constants.UdiEntityType.DataTypeContainer:
                    return UmbracoObjectTypes.DataTypeContainer;
                case Constants.UdiEntityType.MemberType:
                    return UmbracoObjectTypes.MemberType;
                case Constants.UdiEntityType.MemberGroup:
                    return UmbracoObjectTypes.MemberGroup;
                case Constants.UdiEntityType.Stylesheet:
                    return UmbracoObjectTypes.Stylesheet;
                case Constants.UdiEntityType.RelationType:
                    return UmbracoObjectTypes.RelationType;
                case Constants.UdiEntityType.FormsForm:
                    return UmbracoObjectTypes.FormsForm;
                case Constants.UdiEntityType.FormsPreValue:
                    return UmbracoObjectTypes.FormsPreValue;
                case Constants.UdiEntityType.FormsDataSource:
                    return UmbracoObjectTypes.FormsDataSource;
                case Constants.UdiEntityType.Language:
                    return UmbracoObjectTypes.Language;
            }

            throw new NotSupportedException(
                $"EntityType \"{entityType}\" does not have a matching UmbracoObjectType.");
        }
    }
}
