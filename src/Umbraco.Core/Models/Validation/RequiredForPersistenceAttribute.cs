using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Models.Validation;

/// <summary>
///     Specifies that a data field value is required in order to persist an object.
/// </summary>
/// <remarks>
///     <para>
///         There are two levels of validation in Umbraco. (1) value validation is performed by
///         <see cref="IValueValidator" />
///         instances; it can prevent a content item from being published, but not from being saved. (2) required
///         validation
///         of properties marked with <see cref="RequiredForPersistenceAttribute" />; it does prevent an object from being
///         saved
///         and is used for properties that are absolutely mandatory, such as the name of a content item.
///     </para>
/// </remarks>
public class RequiredForPersistenceAttribute : RequiredAttribute
{
    /// <summary>
    ///     Determines whether an object has all required values for persistence.
    /// </summary>
    public static bool HasRequiredValuesForPersistence(object model) =>
        model.GetType().GetProperties().All(x =>
        {
            RequiredForPersistenceAttribute? a = x.GetCustomAttribute<RequiredForPersistenceAttribute>();
            return a == null || a.IsValid(x.GetValue(model));
        });
}
