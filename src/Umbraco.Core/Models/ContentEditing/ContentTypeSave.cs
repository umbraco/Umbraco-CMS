using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Abstract model used to save content types
/// </summary>
[DataContract(Name = "contentType", Namespace = "")]
public abstract class ContentTypeSave : ContentTypeBasic, IValidatableObject
{
    protected ContentTypeSave()
    {
        AllowedContentTypes = new List<int>();
        CompositeContentTypes = new List<string>();
    }

    // Compositions
    [DataMember(Name = "compositeContentTypes")]
    public IEnumerable<string> CompositeContentTypes { get; set; }

    [DataMember(Name = "allowAsRoot")]
    public bool AllowAsRoot { get; set; }

    // Allowed child types
    [DataMember(Name = "allowedContentTypes")]
    public IEnumerable<int> AllowedContentTypes { get; set; }

    [DataMember(Name = "historyCleanup")]
    public HistoryCleanupViewModel? HistoryCleanup { get; set; }

    /// <summary>
    ///     Custom validation
    /// </summary>
    /// <param name="validationContext"></param>
    /// <returns></returns>
    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CompositeContentTypes.Any(x => x.IsNullOrWhiteSpace()))
        {
            yield return new ValidationResult(
                "Composite Content Type value cannot be null",
                new[] { "CompositeContentTypes" });
        }
    }
}

/// <summary>
///     Abstract model used to save content types
/// </summary>
/// <typeparam name="TPropertyType"></typeparam>
[DataContract(Name = "contentType", Namespace = "")]
public abstract class ContentTypeSave<TPropertyType> : ContentTypeSave
    where TPropertyType : PropertyTypeBasic
{
    protected ContentTypeSave() => Groups = new List<PropertyGroupBasic<TPropertyType>>();

    /// <summary>
    ///     A rule for defining how a content type can be varied
    /// </summary>
    /// <remarks>
    ///     This is only supported on document types right now but in the future it could be media types too
    /// </remarks>
    [DataMember(Name = "allowCultureVariant")]
    public bool AllowCultureVariant { get; set; }

    [DataMember(Name = "allowSegmentVariant")]
    public bool AllowSegmentVariant { get; set; }

    // Tabs
    [DataMember(Name = "groups")]
    public IEnumerable<PropertyGroupBasic<TPropertyType>> Groups { get; set; }

    /// <summary>
    ///     Custom validation
    /// </summary>
    /// <param name="validationContext"></param>
    /// <returns></returns>
    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (ValidationResult validationResult in base.Validate(validationContext))
        {
            yield return validationResult;
        }

        foreach (IGrouping<string, PropertyGroupBasic<TPropertyType>> duplicateGroupAlias in Groups
                     .GroupBy(x => x.Alias).Where(x => x.Count() > 1))
        {
            var lastGroupIndex = Groups.IndexOf(duplicateGroupAlias.Last());
            yield return new ValidationResult("Duplicate aliases are not allowed: " + duplicateGroupAlias.Key, new[]
            {
                // TODO: We don't display the alias yet, so add the validation message to the name
                $"Groups[{lastGroupIndex}].Name",
            });
        }

        foreach (IGrouping<(string?, string? Name), PropertyGroupBasic<TPropertyType>> duplicateGroupName in Groups
                     .GroupBy(x => (x.GetParentAlias(), x.Name)).Where(x => x.Count() > 1))
        {
            var lastGroupIndex = Groups.IndexOf(duplicateGroupName.Last());
            yield return new ValidationResult(
                "Duplicate names are not allowed",
                new[] { $"Groups[{lastGroupIndex}].Name" });
        }

        foreach (IGrouping<string, TPropertyType> duplicatePropertyAlias in Groups.SelectMany(x => x.Properties)
                     .GroupBy(x => x.Alias).Where(x => x.Count() > 1))
        {
            TPropertyType lastProperty = duplicatePropertyAlias.Last();
            PropertyGroupBasic<TPropertyType> propertyGroup = Groups.Single(x => x.Properties.Contains(lastProperty));
            var lastPropertyIndex = propertyGroup.Properties.IndexOf(lastProperty);
            var propertyGroupIndex = Groups.IndexOf(propertyGroup);

            yield return new ValidationResult(
                "Duplicate property aliases not allowed: " + duplicatePropertyAlias.Key,
                new[] { $"Groups[{propertyGroupIndex}].Properties[{lastPropertyIndex}].Alias" });
        }
    }
}
