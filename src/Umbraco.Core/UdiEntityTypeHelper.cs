using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core;

/// <summary>
///     Provides helper methods for converting between UDI entity types and Umbraco object types.
/// </summary>
public static class UdiEntityTypeHelper
{
    /// <summary>
    ///     Converts an <see cref="UmbracoObjectTypes" /> value to its corresponding UDI entity type string.
    /// </summary>
    /// <param name="umbracoObjectType">The Umbraco object type to convert.</param>
    /// <returns>The UDI entity type string.</returns>
    /// <exception cref="NotSupportedException">The Umbraco object type does not have a matching entity type.</exception>
    public static string FromUmbracoObjectType(UmbracoObjectTypes umbracoObjectType)
    {
        switch (umbracoObjectType)
        {
            case UmbracoObjectTypes.Document:
                return Constants.UdiEntityType.Document;
            case UmbracoObjectTypes.DocumentBlueprint:
                return Constants.UdiEntityType.DocumentBlueprint;
            case UmbracoObjectTypes.DocumentBlueprintContainer:
                return Constants.UdiEntityType.DocumentBlueprintContainer;
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

    /// <summary>
    ///     Converts a UDI entity type string to its corresponding <see cref="UmbracoObjectTypes" /> value.
    /// </summary>
    /// <param name="entityType">The UDI entity type string to convert.</param>
    /// <returns>The corresponding <see cref="UmbracoObjectTypes" /> value.</returns>
    /// <exception cref="NotSupportedException">The entity type does not have a matching Umbraco object type.</exception>
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
