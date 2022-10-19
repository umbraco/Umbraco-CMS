using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core;

public static class UdiEntityTypeHelper
{
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
